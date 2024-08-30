using Chr.Avro.Abstract;
using Chr.Avro.Confluent;
using Chr.Avro.Representation;
using Chr.Avro.Serialization;
using Confluent.SchemaRegistry;
using Genie.Common.Types;
using Genie.Common.Utils.ChangeFeed;
using Genie.Common.Utils.Cosmos;
using Google.Protobuf;
using Microsoft.IO;
using System.Text;
using ZstdSharp;
using ZstdSharp.Unsafe;

namespace Genie.Common.Utils;

public class AvroOneOf
{
    private readonly BaseRequest? req;

    public AvroOneOf() { }

    public AvroOneOf(BaseRequest req, ChangeLog changeLog)
    {
        this.req = req;
        this.ChangeLog = changeLog;
    }

    public ChangeLog? ChangeLog { get; set; }

    public Types.PartyRequest? PartyRequest
    {
        get => (req is Types.PartyRequest a) ? a : null;
        init => req = (req == null ? value : req); // guard against deserialization overwrites
    }
}

public class AvroTwo
{
    public AvroTwo() { }

    public BaseRequest? BaseRequest { get; set; }
}

public class AvroSupport
{
    public static SchemaRegistryConfig GetSchemaRegistryConfig() => new() { Url = "http://registry:8081" };

    public static SchemaBuilder GetSchemaBuilder() => new(SchemaBuilder.CreateDefaultCaseBuilders(nullableReferenceTypeBehavior: NullableReferenceTypeBehavior.All)
        .Prepend(builder => new NetopoolgyFeatureCollectionSchemaBuilderCase()));

    public static SchemaRegistrySerializerBuilder GetSerializerBuilder(SchemaRegistryConfig registryConfig, SchemaBuilder schemaBuilder) =>
        new (registryConfig, schemaBuilder, serializerBuilder: new BinarySerializerBuilder(
                    BinarySerializerBuilder.CreateDefaultCaseBuilders().Prepend(builder => new NetopoolgyFeatureCollectionSerializerBuilderCase(builder))));

    public static BinarySerializerBuilder GetSerializerBuilder() =>
        new (BinarySerializerBuilder.CreateDefaultCaseBuilders().Prepend(builder => new NetopoolgyFeatureCollectionSerializerBuilderCase(builder)));

    public static BinaryDeserializerBuilder GetBinaryDeserializerBuilder() => new(BinaryDeserializerBuilder.CreateDefaultCaseBuilders()
            .Prepend(builder => new NetopoolgyFeatureCollectionDeserializerBuilderCase(builder)));

    public static T GetTypedMessage<T>(byte[] val, AsyncSchemaRegistryDeserializer<T> deserializer)
    {
        return deserializer.DeserializeAsync(val, false, new Confluent.Kafka.SerializationContext()).GetAwaiter().GetResult();
    }

    public static byte[] TestReadLog<T>(string path)
    {
        using var registry = new CachedSchemaRegistryClient(GetSchemaRegistryConfig());

        var schemaBuilder = GetSchemaBuilder();

        var schema = schemaBuilder.BuildSchema<T>() as UnionSchema;

        var writer = new JsonSchemaWriter();
        File.WriteAllText(@$"C:\temp\logs\{typeof(T).Name}.avsc", writer.Write(schema!));

        var deserializerBuilder = GetBinaryDeserializerBuilder();

        var deser = deserializerBuilder.BuildDelegate<T>(schema!);


        var bytes = File.ReadAllBytes(path);
        var reader = new Chr.Avro.Serialization.BinaryReader(bytes);

        var channels = new List<T>();

        while (reader.Index < bytes.Length)
            channels.Add(deser(ref reader));

        return GenerateAvroContainer(channels, null);
    }

    private static readonly RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();

    public static byte[] GenerateAvroContainer<T>(List<T> channels, int? zStandardLevel = null)
    {
        using var output = manager.GetStream();
        var writer = new Chr.Avro.Serialization.BinaryWriter(output);
        var codedStream = new CodedOutputStream(output);

        // Create OCF

        //////////////////////////////
        // Header
        //////////////////////////////

        // Avro Marker
        var avro = Encoding.UTF8.GetBytes("Obj");
        output.Write(avro);


        // Version
        codedStream.WriteInt32(1); // Version
        codedStream.WriteInt32(avro.Length + 1); // Size of the first block
        codedStream.Flush();

        // schema includes length
        writer.WriteString("avro.schema");


        var schemaBuilder = GetSchemaBuilder();

        var schema = schemaBuilder.BuildSchema<T>() as UnionSchema;
        writer.WriteString(new JsonSchemaWriter().Write(schema!));

        // Codec
        writer.WriteString("avro.codec");
        if (zStandardLevel != null)
            writer.WriteString("zstandard");
        else
            writer.WriteString("null");

        codedStream.WriteInt32(0);
        codedStream.Flush();

        string marker = Guid.NewGuid().ToString("N");
        writer.WriteFixed(marker.Hex());


        // Each item has a marker so 2x
        codedStream.WriteInt32(channels.Count * 2);
        codedStream.Flush();

        var serializerBuilder = new BinarySerializerBuilder(BinarySerializerBuilder.CreateDefaultCaseBuilders()
            .Prepend(builder => new NetopoolgyFeatureCollectionSerializerBuilderCase(builder)));


        var serialize = serializerBuilder.BuildDelegate<T>(schema!);

        using var ms = manager.GetStream();
        var listWriter = new Chr.Avro.Serialization.BinaryWriter(ms);

        foreach (var c in channels)
            serialize(c, listWriter);


        if (zStandardLevel != null)
        {
            using var compressStream = manager.GetStream();
            var compress = new CompressionStream(compressStream, level: zStandardLevel.Value, leaveOpen: false);
            compress.SetParameter(ZSTD_cParameter.ZSTD_c_nbWorkers, Environment.ProcessorCount);
            ms.CopyTo(compress);

            writer.WriteBytes(compressStream.ToArray());
        }
        else
            writer.WriteBytes(ms.ToArray());

        writer.WriteFixed(marker.Hex());


        codedStream.Flush();

        return output.ToArray();
    }
}
