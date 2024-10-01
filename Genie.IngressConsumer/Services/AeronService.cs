﻿using Adaptive.Aeron;
using Adaptive.Agrona;
using Adaptive.Agrona.Concurrent;
using Genie.Adapters.Brokers.Aeron;
using Genie.Adapters.Persistence.Postgres;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using System.Buffers;
using ZLogger;

namespace Genie.IngressConsumer.Services;

public class AeronService
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;

        using var factory = ZloggerFactory.GetFactory(context.Zlogger.Path);
        var logger = factory.CreateLogger("Program");

        await Start(context, logger);

        Console.WriteLine("Aeron exited");
    }


    private static async Task Start(GenieContext context, ILogger logger)
    {
        var schemaBuilder = AvroSupport.GetSchemaBuilder();
        var serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());
        var dict = new Dictionary<string, Publication>();

        var aeroncontext = new Aeron.Context();
        var aeron = Aeron.Connect(aeroncontext);

        var consumer = AeronUtils.SetupSubscriber(aeron, "aeron:udp?endpoint=localhost:40123", 10);

        var timer = new CounterConsoleLogger();
        var pool = new DefaultObjectPool<PostgresPooledObject>(new DefaultPooledObjectPolicy<PostgresPooledObject>());

        while (true)
        {
            try
            {
                using CancellationTokenSource cts = new();


                Console.WriteLine("Starting Aeron Pump: " + cts.Token);
                var pump = AeronPump<byte[]>.Run(
                    consumer,
                    async message =>
                    {
                        timer.Process();

                        var proto = Any.Parser.ParseFrom(message).Unpack<Grpc.PartyBenchmarkRequest>();

                        Console.WriteLine($@"Logged {CounterConsoleLogger.Counter} {proto.Party.CosmosBase.Identifier.Id}");

                        if (!context.Simple)
                            await EventTask.Process(context, proto, logger, pool, cts.Token);

                        using var ms = manager.GetStream();
                        serializer(new EventTaskJob
                        {
                            Id = proto.Request.CosmosBase.Identifier.Id,
                            Job = "Report",
                            Status = EventTaskJobStatus.Completed
                        }, new Chr.Avro.Serialization.BinaryWriter(ms));

                        var exists = dict.TryGetValue(proto.Request.Info, out Publication? producer);

                        if (!exists)
                        {
                            producer = aeron.AddPublication(proto.Request.Info, 10);
                            dict.Add(proto.Request.Info, producer);
                        }

                        var buffer = new UnsafeBuffer(BufferUtil.AllocateDirectAligned(512, BitUtil.CACHE_LINE_LENGTH));
                        var data = ms.GetReadOnlySequence().ToArray();
                        buffer.PutBytes(0, data);

                        while(!producer!.IsConnected)
                            await Task.Delay(500);

                        var result = producer.Offer(buffer, 0, data.Length);

                        if (result < 0L)
                        {
                            switch (result)
                            {
                                case Publication.BACK_PRESSURED:
                                    Console.WriteLine(" Offer failed due to back pressure");
                                    break;
                                case Publication.NOT_CONNECTED:
                                    Console.WriteLine(" Offer failed because publisher is not connected to subscriber");
                                    break;
                                case Publication.ADMIN_ACTION:
                                    Console.WriteLine("Offer failed because of an administration action in the system");
                                    break;
                                case Publication.CLOSED:
                                    Console.WriteLine("Offer failed publication is closed");
                                    break;
                                default:
                                    Console.WriteLine(" Offer failed due to unknown reason");
                                    break;
                            }
                        }
                    },
                    maxDegreeOfParallelism: 32,
                    cts.Token);


                await pump.Completion;
            }

            catch(Exception ex)
            {
                _ = ex;
                timer.ProcessError();
            }
        }

    }
}