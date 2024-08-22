using Genie.Common;
using Genie.Common.Adapters.RabbitMQ;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Buffers;
using ZLogger;


namespace Genie.IngressConsumer.Services;


public class RabbitMQService
{
    private static readonly RecyclableMemoryStreamManager manager = new();

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
        var args = new Dictionary<string, object>();
        args.Add("x-max-length", 10000);

        var context = GenieContext.Build().GenieContext;

        var conn = RabbitUtils.GetConnection(context.RabbitMQ, true);

        var ingressChannel = conn.CreateModel();

        ingressChannel.ExchangeDeclare(context.RabbitMQ.Exchange, ExchangeType.Direct);
        ingressChannel.QueueDeclare(context.RabbitMQ.Queue, false, false, false, args);
        ingressChannel.QueueBind(context.RabbitMQ.Queue, context.RabbitMQ.Exchange, context.RabbitMQ.RoutingKey, null);

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

            var pool = new DefaultObjectPool<PostGisPooledObject>(new DefaultPooledObjectPolicy<PostGisPooledObject>());

            var timerService = new CounterConsoleLogger();

            var pump = RabbitMQPump<byte[]>.Run(
                consumer,
                async message =>
                {
                    try
                    {
                        timerService.Process();

                        var proto = Any.Parser.ParseFrom(message.Body.ToArray()).Unpack<Grpc.PartyBenchmarkRequest>();
                        await EventTask.Process(context, proto, logger, pool, cts.Token);

                        if (!string.IsNullOrEmpty(message.BasicProperties.ReplyTo))
                        {
                            using var ms = manager.GetStream();
                            serializer(new EventTaskJob
                            {
                                Id = proto.Request.CosmosBase.Identifier.Id,
                                Job = "Report",
                                Status = EventTaskJobStatus.Completed
                            }, new Chr.Avro.Serialization.BinaryWriter(ms));

                            channels.EventChannel.BasicPublish(message.BasicProperties.ReplyTo, context.RabbitMQ.RoutingKey, null, ms.GetReadOnlySequence().ToArray());
                        }

                    }
                    catch(Exception ex)
                    {
                        timerService.ProcessError();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error:" + ex.ToString());
                        logger.LogError(ex.ToString());

                        if (!string.IsNullOrEmpty(message.BasicProperties.ReplyTo))
                        {
                            using var ms = manager.GetStream();
                            serializer(new EventTaskJob
                            {
                                Exception = ex.Message,
                                Status = EventTaskJobStatus.Errored
                            }, new Chr.Avro.Serialization.BinaryWriter(ms));

                            channels.EventChannel.BasicPublish(message.BasicProperties.ReplyTo, context.RabbitMQ.RoutingKey, null, ms.GetReadOnlySequence().ToArray());
                        }
                    }
                },
                maxDegreeOfParallelism: 16,
                cts.Token);


            string consumerTag = channels.IngressChannel.BasicConsume(context.RabbitMQ.Queue, true, consumer);
            await pump.Completion;
        }

        catch(Exception ex)
        {
            await File.AppendAllTextAsync(@"c:\temp\rabbiterror.log", ex.ToString());
        }

    }
}