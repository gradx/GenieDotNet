using Adaptive.Aeron;
using Adaptive.Aeron.LogBuffer;
using Adaptive.Agrona;
using Adaptive.Agrona.Concurrent;
using Adaptive.Cluster.Codecs;
using Adaptive.Cluster.Service;
using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Common;
using Genie.Common.Adapters.ActiveMQ;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using System.Buffers;
using ZLogger;

namespace Genie.IngressConsumer.Services
{
    public class EchoService : IClusteredService
    {
        private ICluster _cluster;
        private CounterConsoleLogger timer = new();
        private SchemaBuilder schemaBuilder = AvroSupport.GetSchemaBuilder();
        private BinarySerializer<EventTaskJob> serializer;
        private GenieContext context;
        private DefaultObjectPool<PostGisPooledObject> pool;
        private static readonly RecyclableMemoryStreamManager manager = new();
        private ILogger logger;

        public void OnStart(ICluster cluster, Image snapshotImage)
        {
            Console.WriteLine("OnStart");
            _cluster = cluster;
            serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());
            context = GenieContext.Build().GenieContext;
            pool = new DefaultObjectPool<PostGisPooledObject>(new DefaultPooledObjectPolicy<PostGisPooledObject>());
            using var factory = ZloggerFactory.GetFactory(context.Zlogger.Path);
            logger = factory.CreateLogger("Program");
        }

        public void OnSessionOpen(IClientSession session, long timestampMs)
        {
            Console.WriteLine($"OnSessionOpen: sessionId={session.Id}, timestamp={timestampMs}");
        }

        public void OnSessionClose(IClientSession session, long timestampMs, CloseReason closeReason)
        {
            Console.WriteLine($"OnSessionClose: sessionId={session.Id}, timestamp={timestampMs}");
        }

        public void OnSessionMessage(IClientSession session, long timestampMs, IDirectBuffer buffer, int offset, int length, Header header)
        {
            timer.Process();

            if (buffer.ByteBuffer == null)
            {
                Console.WriteLine("Logged null buffer");
                return;
            }

            var proto = Any.Parser.ParseFrom(buffer.ByteArray).Unpack<Grpc.PartyBenchmarkRequest>();

            Console.WriteLine($@"Logged {CounterConsoleLogger.Counter} {proto.Party.CosmosBase.Identifier.Id}");

            EventTask.Process(context, proto, logger, pool, new CancellationToken()).GetAwaiter().GetResult();

            
            using var ms = manager.GetStream();
            serializer(new EventTaskJob
            {
                Id = proto.Request.CosmosBase.Identifier.Id,
                Job = "Report",
                Status = EventTaskJobStatus.Completed
            }, new Chr.Avro.Serialization.BinaryWriter(ms));



            var buffer2 = new UnsafeBuffer(BufferUtil.AllocateDirectAligned(512, BitUtil.CACHE_LINE_LENGTH));
            var data = ms.GetReadOnlySequence().ToArray();
            buffer2.PutBytes(0, data);
        }

        public void OnTimerEvent(long correlationId, long timestampMs)
        {
            Console.WriteLine($"OnTimerEvent: correlationId={correlationId}, timestamp={timestampMs}");
        }

        public void OnTakeSnapshot(ExclusivePublication snapshotPublication)
        {
            Console.WriteLine("OnTakeSnapshot");
        }

        public void OnRoleChange(ClusterRole newRole)
        {
            Console.WriteLine($"OnRoleChange: newRole={newRole}");
        }

        public void OnTerminate(ICluster cluster)
        {
            Console.WriteLine("OnTerminate");
        }

        public void OnNewLeadershipTermEvent(
            long leadershipTermId,
            long logPosition,
            long timestamp,
            long termBaseLogPosition,
            int leaderMemberId,
            int logSessionId,
            ClusterTimeUnit timeUnit,
            int appVersion)
        {
            Console.WriteLine($"OnNewLeadershipTerm: leadershipTermId={leadershipTermId}");
        }

        public int DoBackgroundWork(long nowNs)
        {
            return 0;
        }
    }

    public class AeronService2
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

            var clustercontext = new ClusteredServiceContainer.Context().ClusteredService(new EchoService());

            var clustercontainer = ClusteredServiceContainer.Launch(clustercontext);


            var timer = new CounterConsoleLogger();
            var pool = new DefaultObjectPool<PostGisPooledObject>(new DefaultPooledObjectPolicy<PostGisPooledObject>());

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

                            while (!producer.IsConnected)
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

                catch (Exception ex)
                {
                    _ = ex;
                    timer.ProcessError();
                }
            }

        }
    }
}
