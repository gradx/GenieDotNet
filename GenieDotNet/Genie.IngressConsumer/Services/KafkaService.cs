using Chr.Avro.Confluent;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Genie.Common;
using Genie.Common.Adapters.Kafka;
using Genie.Common.Types;
using Genie.Common.Utils;
using Microsoft.Extensions.Logging;
using System.Text;
using ZLogger;

namespace Genie.IngressConsumer.Services
{
    public class KafkaService
    {
        public static async Task Start()
        {
            var context = GenieContext.Build().GenieContext;



            var config = KafkaUtils.GetConfig(context);

            using var factory = ZloggerFactory.GetFactory(@"C:\temp\logs");
            var logger = factory.CreateLogger("Program");

            Console.WriteLine("Starting Kafka...");
            await Kafka(config, context, logger);

            Console.WriteLine("All tasks exited");
        }

        public static async Task Kafka(ConsumerConfig config, GenieContext context, ILogger logger)
        {
            var schemaBuilder = AvroSupport.GetSchemaBuilder();

            var consumerBuilder = new ConsumerBuilder<string, byte[]>(config);

            using var registry = new CachedSchemaRegistryClient(AvroSupport.GetSchemaRegistryConfig());
            consumerBuilder.SetAvroKeyDeserializer(registry);

            using var consumer = consumerBuilder.Build();
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = context.Kafka.Host }).Build();
            await KafkaUtils.CreateTopic(adminClient, [context.Kafka.Ingress]);
            await KafkaUtils.CreateTopic(adminClient, [context.Kafka.Events]);

            await Task.Delay(1000);
            consumer.Subscribe(context.Kafka.Ingress);
            var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();

            var registryConfig = AvroSupport.GetSchemaRegistryConfig();

            var producerBuilder = new ProducerBuilder<string, EventTaskJob>(new ProducerConfig { BootstrapServers = context.Kafka.Host });
            await Task.WhenAll(
                producerBuilder.SetAvroKeySerializer(registry,
                    $"{typeof(EventTaskJob).FullName}-Key", registerAutomatically: AutomaticRegistrationBehavior.Always),
                producerBuilder.SetAvroValueSerializer(AvroSupport.GetSerializerBuilder(registryConfig, schemaBuilder),
                    $"{typeof(EventTaskJob).FullName}-Value", registerAutomatically: AutomaticRegistrationBehavior.Always)
            );

            //Use name as shortcut
            using var producer = producerBuilder.Build();

            var deserializer = new AsyncSchemaRegistryDeserializer<PartyRequest>(registry, deserializerBuilder);

            while (true)
            {
                try
                {
                    using CancellationTokenSource cts = new();
                    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
                    var errors = 0;
                    while (true)
                    {
                        try
                        {
                            Console.WriteLine("Starting Kafka2 Pump: " + cts.Token);


                            var result = consumer.Consume(cts.Token);
                            //Console.WriteLine("Message Received: " + result.Message.Key);
                            //continue;

                            var ackCounter = 0;
                            var pump = KafkaPump<string, byte[]>.Run(
                                consumer,
                                async message =>
                                {
                                    ackCounter++;
                                    Console.WriteLine($@"Message Received: {ackCounter}");
                                    //return;

                                    BaseRequest req = message.Key switch
                                    {
                                        "PartyRequest" => AvroSupport.GetTypedMessage(message.Value, deserializer),
                                        _ => throw new TypeLoadException($"Type not mapped: {message.Key}")
                                    };

                                    await EventTask.Process(context, req, logger, cts.Token);

                                    var eventChannel = message.Headers.FirstOrDefault(t => t.Key == "EventChannel")!;

                                    if (eventChannel != null)
                                    {
                                        var topic = Encoding.UTF8.GetString(eventChannel.GetValueBytes());
                                        await KafkaUtils.Post(producer, topic, new EventTaskJob { Id = req.Id, Job = "Report" }, cts.Token);
                                    }
                                },
                                maxDegreeOfParallelism: 16,
                                cts.Token);

                            //pump.Stop();

                            await pump.Completion;

                            errors = 0;
                        }
                        catch (Exception ex)
                        {
                            _ = ex;
                            errors++;
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
