using Genie.Common;
using Genie.Common.Adapters.RabbitMQ;
using Genie.Common.Types;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ZLogger;


namespace Genie.IngressConsumer.Services;

public class RabbitMQService
{
    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;


        using var factory = ZloggerFactory.GetFactory(@"C:\temp\logs");
        var logger = factory.CreateLogger("Program");


        await RabbitMq(context, logger);
        Console.WriteLine("RabbitMQ exited");
    }

    public static (IModel IngressChannel, IModel EventChannel) Channels()
    {
        var context = GenieContext.Build().GenieContext;

        var conn = RabbitUtils.GetConnection(context.Rabbit, true);

        var ingressChannel = conn.CreateModel();

        ingressChannel.ExchangeDeclare(context.Rabbit.Exchange, ExchangeType.Direct);
        ingressChannel.QueueDeclare(context.Rabbit.Queue, false, false, false, null);
        ingressChannel.QueueBind(context.Rabbit.Queue, context.Rabbit.Exchange, context.Rabbit.RoutingKey, null);

        var eventChannel = conn.CreateModel();
        return (ingressChannel, eventChannel);
    }


    public static async Task RabbitMq(GenieContext context, ILogger logger)
    {
        try
        {
            var schemaBuilder = AvroSupport.GetSchemaBuilder();
            var serializer = AvroSupport.GetSerializerBuilder().BuildDelegate<EventTaskJob>(schemaBuilder.BuildSchema<EventTaskJob>());

            using CancellationTokenSource cts = new();
            Console.WriteLine("Starting RabbitMQ Pump: " + cts.Token);

            var channels = Channels();

            var consumer = new AsyncEventingBasicConsumer(channels.IngressChannel);
            var ackCounter = 0;

            //var deserializer = new ProtobufDeserializer<Grpc.PartyRequest>();

            var pump = RabbitMQPump<byte[]>.Run(
                consumer,
                async message =>
                {
                    try
                    {
                        ackCounter++;
                        Console.WriteLine($@"received {ackCounter}");
                        //var proto = await deserializer.DeserializeAsync(message.Body,
                        //    false,
                        //    new Confluent.Kafka.SerializationContext());

                        var proto = Any.Parser.ParseFrom(message.Body.ToArray()).Unpack<Grpc.PartyRequest>();

                        await EventTask.Process(context, proto, logger, cts.Token);

                        if (!string.IsNullOrEmpty(message.BasicProperties.ReplyTo))
                        {
                            var ms = new MemoryStream();
                            serializer(new EventTaskJob
                            {
                                Id = proto.Request.CosmosBase.Identifier.Id,
                                Job = "Report",
                                Status = EventTaskJobStatus.Completed
                            }, new Chr.Avro.Serialization.BinaryWriter(ms));

                            channels.EventChannel.BasicPublish(message.BasicProperties.ReplyTo, context.Rabbit.RoutingKey, null, ms.ToArray());
                        }

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error:" + ex.ToString());
                        logger.LogError(ex.ToString());

                        if (!string.IsNullOrEmpty(message.BasicProperties.ReplyTo))
                        {
                            var ms = new MemoryStream();
                            serializer(new EventTaskJob
                            {
                                Exception = ex.Message,
                                Status = EventTaskJobStatus.Errored
                            }, new Chr.Avro.Serialization.BinaryWriter(ms));

                            channels.EventChannel.BasicPublish(message.BasicProperties.ReplyTo, context.Rabbit.RoutingKey, null, ms.ToArray());
                        }
                    }
                },
                maxDegreeOfParallelism: 16,
                cts.Token);


            string consumerTag = channels.IngressChannel.BasicConsume(context.Rabbit.Queue, true, consumer);
            await pump.Completion;

            //pump.Stop();

            //if (pump.Completion.IsCanceled)
            //{
            //    channels.IngressChannel.BasicCancel(consumerTag);
            //    channels.IngressChannel.Close();
            //    channels.EventChannel.Close();

            //    //var reset = Channels();
            //    //ingress = reset.IngressChannel;
            //    //events = reset.EventChannel;
            //}
        }

        catch(Exception ex)
        {
            await File.AppendAllTextAsync(@"c:\temp\rabbiterror.log", ex.ToString());
        }

    }
}