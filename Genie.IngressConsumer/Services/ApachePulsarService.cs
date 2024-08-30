using Chr.Avro.Serialization;
using DotPulsar;
using DotPulsar.Extensions;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Common.Utils.Cosmos;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using System.Buffers;
using ZLogger;


namespace Genie.IngressConsumer.Services;

public class ApachePulsarService
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;

        using var factory = ZloggerFactory.GetFactory(@"C:\temp\logs");
        var logger = factory.CreateLogger("Program");

        Console.WriteLine("Starting Apache Pulsar System...");
        await Pulsar(context, logger);

        Console.WriteLine("Apache Pulsar exited");
    }

    public async static Task Pulsar(GenieContext context, ILogger logger)
    {
        var client = PulsarClient.Builder().ServiceUrl(new Uri("pulsar://pulsar:6650")).Build();

        var consumer = client.NewConsumer(Schema.ByteArray)
            .Topic(context.Kafka.Ingress + "Apache")
            .SubscriptionName("ActorConsumer")
            .Create();

        var schemaBuilder = AvroSupport.GetSchemaBuilder();

        var serializerBuilder = new BinarySerializerBuilder(BinarySerializerBuilder.CreateDefaultCaseBuilders()
            .Prepend(builder => new NetopoolgyFeatureCollectionSerializerBuilderCase(builder)));

        var serialize = serializerBuilder.BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());

        var pool = new DefaultObjectPool<PostGisPooledObject>(new DefaultPooledObjectPolicy<PostGisPooledObject>());

        while (true)
        {
            using CancellationTokenSource cts = new();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
            var errors = 0;
            while (true)
            {
                try
                {
                    //Console.WriteLine("Starting Actor Consumer: " + cts.Token);

                    var cr = await consumer.Receive(cts.Token);

                    if (string.IsNullOrEmpty(cr.Key))
                        continue;

                    var request = cr.Value();

                    //var proto = await new ProtobufDeserializer<Grpc.PartyRequest>().DeserializeAsync(request.ToArray(), false, new Confluent.Kafka.SerializationContext());

                    var proto = Any.Parser.ParseFrom(request.ToArray()).Unpack<Grpc.PartyRequest>();

                    await EventTask.Process(context, proto, logger, pool, cts.Token);

                    var producer = client.NewProducer(Schema.ByteArray)
                        .Topic(proto.Request.CosmosBase.Identifier.Id)
                        .Create();

                    using var ms = manager.GetStream();

                    serialize(new EventTaskJob { Id = proto.Request.CosmosBase.Identifier.Id, Job = "Report" },
                        new Chr.Avro.Serialization.BinaryWriter(ms));

                    await producer.Send(ms.GetReadOnlySequence().ToArray());

                    await consumer.Acknowledge(cr.MessageId);

                    errors = 0;
                }
                catch (Exception ex)
                {
                    _ = ex;
                    errors++;
                }
            }
        }
    }
}