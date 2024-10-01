using Genie.Adapters.Brokers.NATS;
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
using NATS.Client.Core;
using System.Buffers;
using ZLogger;

namespace Genie.IngressConsumer.Services;

public class NatsService
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;

        using var factory = ZloggerFactory.GetFactory(context.Zlogger.Path);
        var logger = factory.CreateLogger("Program");

        await Start(context, logger);

        Console.WriteLine("NATS exited");
    }


    private static async Task Start(GenieContext context, ILogger logger)
    {
        var schemaBuilder = AvroSupport.GetSchemaBuilder();
        var serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());

        var connection = new NatsConnection();
        var timer = new CounterConsoleLogger();
        var pool = new DefaultObjectPool<PostgresPooledObject>(new DefaultPooledObjectPolicy<PostgresPooledObject>());

        while (true)
        {
            try
            {
                using CancellationTokenSource cts = new();

                Console.WriteLine("Starting NATS Pump: " + cts.Token);
                var pump = NatsPump<byte[]>.Run(
                    connection,
                    async message =>
                    {
                        timer.Process();

                        var proto = Any.Parser.ParseFrom(message).Unpack<Grpc.PartyBenchmarkRequest>();

                        if (!context.Simple)
                            await EventTask.Process(context, proto, logger, pool, cts.Token);

                        using var ms = manager.GetStream();
                        serializer(new EventTaskJob
                        {
                            Id = proto.Request.CosmosBase.Identifier.Id,
                            Job = "Report"
                        }, new Chr.Avro.Serialization.BinaryWriter(ms));

                        await connection.PublishAsync<byte[]>(subject: proto.Request.Info, data: ms.GetReadOnlySequence().ToArray());

                        await Task.CompletedTask;

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