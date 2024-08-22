using Genie.Common.Adapters.RabbitMQ;
using Genie.Common.Utils;
using Genie.Common;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLogger;
using Genie.Common.Types;
using Confluent.SchemaRegistry.Serdes;
using Apache.NMS;
using Apache.NMS.Util;
using Genie.Common.Adapters.ActiveMQ;
using DotPulsar.Internal;
using Apache.NMS.ActiveMQ;
using Proto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.IO;
using System.Buffers;
using Genie.Common.Performance;
using Microsoft.Extensions.ObjectPool;

namespace Genie.IngressConsumer.Services;

public class ActiveMQService
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;

        using var factory = ZloggerFactory.GetFactory(context.Zlogger.Path);
        var logger = factory.CreateLogger("Program");

        await ActiveMQ(context, logger);

        Console.WriteLine("ActiveMQ exited");
    }


    public static async Task ActiveMQ(GenieContext context, ILogger logger)
    {
        var schemaBuilder = AvroSupport.GetSchemaBuilder();
        var serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());
        var dict = new Dictionary<string, IMessageProducer>();

        Uri connecturi = new(context.ActiveMQ.ConnectionString);
        var factory = new NMSConnectionFactory(connecturi);
        Apache.NMS.IConnection connection = factory.CreateConnection(context.ActiveMQ.Username, context.ActiveMQ.Password);
        using ISession ingressSession = connection.CreateSession();; //
        using IDestination destination = SessionUtil.GetDestination(ingressSession, context.ActiveMQ.Ingress);
        using IMessageConsumer consumer = ingressSession.CreateConsumer(destination);

        Apache.NMS.IConnection egressConn = factory.CreateConnection(context.ActiveMQ.Username, context.ActiveMQ.Password);
        ISession egressSession = egressConn.CreateSession();

        var timer = new CounterConsoleLogger();
        var pool = new DefaultObjectPool<PostGisPooledObject>(new DefaultPooledObjectPolicy<PostGisPooledObject>());

        while (true)
        {
            try
            {
                using CancellationTokenSource cts = new();

                if(!egressConn.IsStarted)
                    egressConn.Start();

                Console.WriteLine("Starting ActiveMQ Pump: " + cts.Token);
                var pump = ActiveMQPump<byte[]>.Run(
                    consumer,
                    connection,
                    async message =>
                    {
                        timer.Process();

                        var proto = Any.Parser.ParseFrom(((IBytesMessage)message).Content).Unpack<Grpc.PartyBenchmarkRequest>();

                        await EventTask.Process(context, proto, logger, pool, cts.Token);

                        using var ms = manager.GetStream();
                        serializer(new EventTaskJob
                        {
                            Id = proto.Request.CosmosBase.Identifier.Id,
                            Job = "Report"
                        }, new Chr.Avro.Serialization.BinaryWriter(ms));

                        var exists = dict.TryGetValue(message.NMSCorrelationID, out IMessageProducer? producer);

                        if (!exists)
                        {
                            IDestination destination = SessionUtil.GetDestination(egressSession, $@"queue://{message.NMSCorrelationID}");// DestinationType.TemporaryQueue);
                            producer = egressSession.CreateProducer(destination);
                            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                            dict.Add(message.NMSCorrelationID, producer);
                        }

                        var msg = egressSession.CreateBytesMessage(ms.GetReadOnlySequence().ToArray());
                        producer!.Send(msg);

                        await Task.CompletedTask;

                    },
                    maxDegreeOfParallelism: 32,
                    cts.Token);

                if(!connection.IsStarted)
                    connection.Start();

                await pump.Completion;
            }

            catch(Exception ex)
            {
                _ = ex;
                timer.ProcessError();
                //egressConn.Stop();
                //connection.Stop();
            }
        }

    }
}