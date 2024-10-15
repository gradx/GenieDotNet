using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Adapters.Serializers.Avro;
using Genie.Core;
using NATS.Client.Core;

namespace Genie.Adapters.Brokers.NATS;

public class NatsPooledObject<T> : GeniePooledObject
{
    public NatsConnection NatsConnection { get; set; }

    public BinaryDeserializer<T> Deserializer { get; set; }

    public T? Result { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);


    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {
        NatsConnection = new NatsConnection();

        _ = Task.Run(async () => {
            await foreach (var msg in NatsConnection.SubscribeAsync<byte[]>(subject: EventChannel))
            {
                Result = Deserialize(msg.Data!);
                ReceiveSignal.Set();
            }
        });

        var schema = schemaBuilder.BuildSchema<T>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<T>(schema);
    }

    public T Deserialize(byte[] help)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(help);
        return Deserializer(ref reader);
    }
}