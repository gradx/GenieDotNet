using Chr.Avro.Confluent;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Cysharp.IO;
using Genie.Adapters.Brokers.Kafka;
using Genie.Adapters.Persistence.Postgres;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using System.Text;
using ZLogger;
using static Genie.Common.Adapters.CosmosAdapter;

namespace Genie.IngressConsumer.Services
{
    public class KafkaService
    {
        private static readonly RecyclableMemoryStreamManager manager = new();

        public static async Task Start()
        {
            var context = GenieContext.Build().GenieContext;

            using var factory = ZloggerFactory.GetFactory(@"C:\temp\logs");
            var logger = factory.CreateLogger("Program");

            Console.WriteLine("Starting Kafka...");
            await Kafka(context, logger);

            Console.WriteLine("All tasks exited");
        }

        public static async Task Kafka(GenieContext context, ILogger logger)
        {
            var config = KafkaUtils.GetConfig(context);

            var schemaBuilder = AvroSupport.GetSchemaBuilder();
            var consumerBuilder = new ConsumerBuilder<string, byte[]>(config);

            using var registry = new CachedSchemaRegistryClient(AvroSupport.GetSchemaRegistryConfig());
            consumerBuilder.SetAvroKeyDeserializer(registry);


            using var consumer = consumerBuilder.Build();
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = context.Kafka.Host}).Build();
            await KafkaUtils.CreateTopic(adminClient, [context.Kafka.Ingress]);
            await KafkaUtils.CreateTopic(adminClient, [context.Kafka.Events]);

            await Task.Delay(1000);
            consumer.Subscribe(context.Kafka.Ingress);
            var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();

            var registryConfig = AvroSupport.GetSchemaRegistryConfig();

            var producerBuilder = new ProducerBuilder<string, EventTaskJob>(new ProducerConfig { BootstrapServers = context.Kafka.Host});
            await Task.WhenAll(
                producerBuilder.SetAvroKeySerializer(registry,
                    $"{typeof(EventTaskJob).FullName}-Key", registerAutomatically: AutomaticRegistrationBehavior.Always),
                producerBuilder.SetAvroValueSerializer(AvroSupport.GetSerializerBuilder(registryConfig, schemaBuilder),
                    $"{typeof(EventTaskJob).FullName}-Value", registerAutomatically: AutomaticRegistrationBehavior.Always)
            );

            //Use name as shortcut
            using var producer = producerBuilder.Build();

            var deserializer = new AsyncSchemaRegistryDeserializer<PartyRequest>(registry, deserializerBuilder);
            var pool = new DefaultObjectPool<PostgresPooledObject>(new DefaultPooledObjectPolicy<PostgresPooledObject>());

            var timer = new CounterConsoleLogger();

            while (true)
            {
                try
                {
                    using CancellationTokenSource cts = new();
                    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
                    
                    while (true)
                    {
                        try
                        {
                            Console.WriteLine("Starting Kafka Pump: " + cts.Token);

                            var pump = KafkaPump<string, byte[]>.Run(
                                consumer,
                                async message =>
                                {
                                    timer.Process();
                                    //return;

                                    BaseRequest req = message.Key switch
                                    {
                                        "PartyRequest" => AvroSupport.GetTypedMessage(message.Value, deserializer),
                                        _ => throw new TypeLoadException($"Type not mapped: {message.Key}")
                                    };

                                    if (!context.Simple)
                                        await EventTask.PartyRequestBenchmark((PartyRequest)req, pool);

                                    var eventChannel = message.Headers.FirstOrDefault(t => t.Key == "EventChannel")!;

                                    if (eventChannel != null)
                                    {
                                        using var ms = manager.GetStream();
                                        await ms.WriteAsync(eventChannel.GetValueBytes());
                                        ms.Position = 0;
                                        Utf8StreamReader reader = new Utf8StreamReader(ms);
                                        var topic = reader.AsTextReader().ReadToEndAsync().Result;

                                        //var topic = Encoding.UTF8.GetString(eventChannel.GetValueBytes());
                                        await KafkaUtils.Post(producer, topic, new EventTaskJob { Id = req.Id, Job = "Report", Status = EventTaskJobStatus.Completed }, cts.Token);
                                    }
                                },
                                maxDegreeOfParallelism: 32,
                                cts.Token);

                            await pump.Completion;
                        }
                        catch (Exception ex)
                        {
                            _ = ex;
                            timer.ProcessError();
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    _ = ex;
                    consumer.Close();
                }
            }
        }

        public static async Task PreconditionTopics(int start, int count)
        {
            var context = GenieContext.Build().GenieContext;
            var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = context.Kafka.Host }).Build();
            List<string> topics = [];

            for (int i = start; i <= count; i++)
                topics.Add(i.ToString());

            await KafkaUtils.CreateTopic(adminClient, [.. topics]);
        }
    }
}
