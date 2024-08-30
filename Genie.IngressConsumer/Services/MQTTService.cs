using Chr.Avro.Confluent;
using Chr.Avro.Serialization;
using Confluent.SchemaRegistry;
using Genie.Common;
using Genie.Common.Adapters.ActiveMQ;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using NetMQ;
using NetMQ.Sockets;
using System.Buffers;
using ZLogger;

namespace Genie.IngressConsumer.Services;
public class MQTTService
{
    public BinaryDeserializer<EventTaskJob> Deserializer { get; set; }
    public GenieContext Context { get; set; }
    public ILogger Logger { get; set;  }

    public RoutingKey RoutingKey { get; set; }
    private static readonly RecyclableMemoryStreamManager manager = new();

    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;

        using var factory = ZloggerFactory.GetFactory(context.Zlogger.Path);
        var logger = factory.CreateLogger("Program");



        var service = new MQTTService();
        service.Logger = logger;
        service.Context = context;
        await service.Run();

        Console.WriteLine($@"MQTTService exited");
    }


    public async Task Run()
    {
        var mqttFactory = new MqttFactory();
        using var mqttClient = mqttFactory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883) // Port is optional
            .Build();

        await mqttClient.ConnectAsync(options, CancellationToken.None);

        using var registry = new CachedSchemaRegistryClient(AvroSupport.GetSchemaRegistryConfig());
        var schemaBuilder = AvroSupport.GetSchemaBuilder();
        var serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());

        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        var deserializer = new AsyncSchemaRegistryDeserializer<PartyRequest>(registry, deserializerBuilder);
        var pool = new DefaultObjectPool<PostGisPooledObject>(new DefaultPooledObjectPolicy<PostGisPooledObject>());

        var timer = new CounterConsoleLogger();

        Dictionary<string, DealerSocket> dict = [];


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
                        Console.WriteLine("Starting MQTTService Pump: " + cts.Token);

                        var pump = MQTTPump<byte[]>.Run(
                            mqttClient,
                            async message =>
                            {
                                timer.Process();

                                //return;

                                var proto = Any.Parser.ParseFrom(message).Unpack<Grpc.PartyBenchmarkRequest>();
                                //Console.WriteLine($@"Logged {proto.Party.CosmosBase.Identifier.Id}");

                                if (!Context.Simple)
                                    await EventTask.Process(Context, proto, Logger, pool, cts.Token);

                                var eventChannel = proto.Request.Info!;

                                using var ms = manager.GetStream();
                                serializer(new EventTaskJob
                                {
                                    Id = eventChannel,
                                    Job = "Report"
                                }, new Chr.Avro.Serialization.BinaryWriter(ms));


                                var message2 = new MqttApplicationMessageBuilder()
                                    .WithTopic(eventChannel)
                                    .WithPayload(ms.GetReadOnlySequence().ToArray())
                                .Build();

                                mqttClient.PublishAsync(message2, CancellationToken.None).GetAwaiter().GetResult();
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
                await mqttClient.DisconnectAsync();
            }
        }
    }
}