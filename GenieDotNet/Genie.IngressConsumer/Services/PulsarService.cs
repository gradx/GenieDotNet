using Genie.Common;
using Microsoft.Extensions.Logging;
using ZLogger;
using Pulsar.Client.Api;
using Pulsar.Client.Common;
using System.Text;
using Genie.Common.Adapters.Pulsar;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.ObjectPool;
using Genie.Common.Performance;
using Genie.Common.Types;
using Microsoft.IO;
using System.Buffers;
using Apache.NMS;

namespace Genie.IngressConsumer.Services;

public class PulsarService
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;


        using var factory = ZloggerFactory.GetFactory(@"C:\temp\logs");
        var logger = factory.CreateLogger("Program");

        Console.WriteLine("Starting Pulsar System...");
        var pulsar = Pulsar(context, logger);

        await Task.WhenAll([pulsar]);

        Console.WriteLine("Pulsar exited");
    }

    public static async Task Pulsar(GenieContext context, ILogger logger)
    {
        var client = await new PulsarClientBuilder()
            .ServiceUrl(context.Pulsar.ConnectionString)
            .BuildAsync();

        var consumer = await client.NewConsumer()
            .Topic(context.Kafka.Ingress)
            .SubscriptionName("ActorConsumer")
            .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
            .SubscribeAsync();

        using CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        var timer = new CounterConsoleLogger();
        var pool = new DefaultObjectPool<PostGisPooledObject>(new DefaultPooledObjectPolicy<PostGisPooledObject>());

        var schemaBuilder = AvroSupport.GetSchemaBuilder();
        var serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());

        var dict = new Dictionary<string, IProducer<byte[]>>();

        while (true)
        {
            try
            {
                Console.WriteLine("Starting Pulsar Pump: " + cts.Token);
                
                var pump = PulsarPump<byte[]>.Run(
                    consumer,
                    async message =>
                    {

                        timer.Process();
                        
                        if (string.IsNullOrEmpty(message.Key))
                        {
                            await consumer.AcknowledgeAsync(message.MessageId);
                            return;
                        }

                        var proto = Any.Parser.ParseFrom(message.Data).Unpack<Grpc.PartyBenchmarkRequest>();

                        await EventTask.Process(context, proto, logger, pool, cts.Token);

                        var exists = dict.TryGetValue(message.Key, out IProducer<byte[]>? producer);

                        if (!exists)
                        {
                            producer = await client.NewProducer()
                                .Topic(message.Key)
                                .CreateAsync();

                            dict.Add(message.Key, producer);
                        }

                        using var ms = manager.GetStream();
                        serializer(new EventTaskJob
                        {
                            Id = proto.Request.CosmosBase.Identifier.Id,
                            Job = "Report",
                            Status = EventTaskJobStatus.Completed
                        }, new Chr.Avro.Serialization.BinaryWriter(ms));

                        await producer!.SendAsync(ms.GetReadOnlySequence().ToArray());

                        await consumer.AcknowledgeAsync(message.MessageId);

                        //await producer.DisposeAsync();
                    },
                    maxDegreeOfParallelism: 32,
                    cts.Token);

                ////pump.Stop();

                await pump.Completion;
            }
            catch (Exception ex)
            {
                _ = ex;
                timer.ProcessError();
            }
        }
    }
}