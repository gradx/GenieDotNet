using Chr.Avro.Confluent;
using Chr.Avro.Serialization;
using Confluent.SchemaRegistry;
using Genie.Adapters.Brokers.ZeroMQ;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using NetMQ;
using NetMQ.Sockets;
using System.Buffers;
using ZLogger;

namespace Genie.IngressConsumer.Services;
public class ZeroMQService
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



        var service = new ZeroMQService
        {
            Logger = logger,
            Context = context
        };
        await service.Run();

        Console.WriteLine($@"ZeroMQ exited");
    }


    public async Task Run()
    {
        using var consumer = new DealerSocket(">tcp://127.0.0.1:5555"); // Working
        //using var server = new DealerSocket("@tcp://127.0.0.1:7777");

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
                        Console.WriteLine("Starting ZeroMQ Pump: " + cts.Token);

                        var pump = ZeroMQPump<byte[]>.Run(
                            consumer,
                            async message =>
                            {
                                timer.Process();

                                // fire and forget only
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

                                var exists = dict.TryGetValue(eventChannel, out DealerSocket? producer);

                                if (!exists)
                                {
                                    producer = new DealerSocket($@"tcp://127.0.0.1:{eventChannel}");
                                    dict.Add(eventChannel, producer);
                                }

                                producer?.SendFrame(ms.GetReadOnlySequence().ToArray());
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

        //while (true)
        //{
        //    var (routingKey, more) = await server.ReceiveRoutingKeyAsync();
        //    var (message, _) = await server.ReceiveFrameStringAsync();

        //    // TODO: process message

        //    await Task.Delay(100);
        //    server.SendMoreFrame(routingKey);
        //    server.SendFrame("Welcome");
        //}
    }


    //async Task ServerAsync()
    //{
    //    var producer = new RouterSocket("inproc://async");
    //    {
    //        //producer.Options.SendHighWatermark = 1000;
    //        //producer.Bind("tcp://*:12345");
    //    }
    //    Consumer = producer;

       
    //}

    //async Task ClientAsync()
    //{


    //    // ">tcp://localhost:12346"
    //    var consumer = new DealerSocket("inproc://async");
    //    {
    //        //consumer.Options.ReceiveHighWatermark = 1000;
    //        //consumer.Connect("tcp://localhost:12346");
    //        //consumer.Subscribe(EventChannel);
    //    }
    //    Producer = consumer;


    //}


}