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

namespace Genie.IngressConsumer.Services;

public class ActiveMQService
{
    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;

        await ActiveMQ(context);

        Console.WriteLine("RabbitMQ exited");
    }


    public static async Task ActiveMQ(GenieContext context)
    {
        var schemaBuilder = AvroSupport.GetSchemaBuilder();
        var serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());
        var dict = new Dictionary<string, IMessageProducer>();
        var ackCounter = 0;
        Uri connecturi = new("activemq:tcp://localhost:61616");
        var factory = new NMSConnectionFactory(connecturi);
        Apache.NMS.IConnection connection = factory.CreateConnection("artemis", "artemis");
        using ISession ingressSession = connection.CreateSession();; //
        using IDestination destination = SessionUtil.GetDestination(ingressSession, "queue://FOO.BAR");
        using IMessageConsumer consumer = ingressSession.CreateConsumer(destination);

        Apache.NMS.IConnection egressConn = factory.CreateConnection("artemis", "artemis");
        ISession egressSession = egressConn.CreateSession();

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
                        ackCounter++;
                        Console.WriteLine($@"message received {ackCounter}");
                        //var proto = await new ProtobufDeserializer<Grpc.PartyRequest>().DeserializeAsync(((IBytesMessage)message).Content,
                        //    false,
                        //    new Confluent.Kafka.SerializationContext());

                        var proto = Any.Parser.ParseFrom(((IBytesMessage)message).Content).Unpack<Grpc.PartyRequest>();

                        ////await EventTask.Process(context, proto, logger, cts.Token);

                        var ms = new MemoryStream();
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
                            //producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                            dict.Add(message.NMSCorrelationID, producer);
                        }

                        var msg = egressSession.CreateBytesMessage(ms.ToArray());
                        producer!.Send(msg);

                        await Task.CompletedTask;

                    },
                    maxDegreeOfParallelism: 16,
                    cts.Token);

                if(!connection.IsStarted)
                    connection.Start();

                await pump.Completion;
            }

            catch(Exception ex)
            {
                _ = ex;
                //egressConn.Stop();
                //connection.Stop();
            }
        }

    }
}