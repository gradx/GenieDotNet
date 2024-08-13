using Genie.Common;
using Microsoft.Extensions.Logging;
using ZLogger;
using Pulsar.Client.Api;
using Pulsar.Client.Common;
using System.Text;
using Genie.Common.Adapters.Pulsar;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;

namespace Genie.IngressConsumer.Services;

public class PulsarService
{
    public static async Task Start()
    {
        var context = GenieContext.Build().GenieContext;


        using var factory = ZloggerFactory.GetFactory(@"C:\temp\logs");
        var logger = factory.CreateLogger("Program");

        Console.WriteLine("Starting Pulsar System...");
        var pulsar = Pulsar(context, logger);

        await Task.WhenAll([pulsar]);

        Console.WriteLine("Pulsar exited");
    }

    public static async Task Pulsar(GenieContext context, ILogger logger)
    {
        var client = await new PulsarClientBuilder()
            .ServiceUrl("pulsar://pulsar:6650")
            .BuildAsync();

        var consumer = await client.NewConsumer()
            .Topic(context.Kafka.Ingress)
            .SubscriptionName("ActorConsumer")
            .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
            .SubscribeAsync();

        using CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
        var errors = 0;
        while (true)
        {
            try
            {
                Console.WriteLine("Starting Pulsar Pump: " + cts.Token);
                var ackCounter = 0;
                var pump = PulsarPump<byte[]>.Run(
                    consumer,
                    async message =>
                    {
                        ackCounter++;
                        Console.WriteLine($@"Received Message {ackCounter}");
                        if (string.IsNullOrEmpty(message.Key))
                        {
                            await consumer.AcknowledgeAsync(message.MessageId);
                            return;
                        }

                        var proto = Any.Parser.ParseFrom(message.Data).Unpack<Grpc.PartyRequest>();

                        await EventTask.Process(context, proto, logger, cts.Token);


                        var producer = await client.NewProducer()
                            .Topic(message.Key)
                            .CreateAsync();

                        await producer.SendAsync(Encoding.UTF8.GetBytes("Done"));

                        //await producer.SendAsync(new EventTaskJob
                        //{
                        //    Id = proto.Request.CosmosBase.Identifier.Id,
                        //    Job = "Report"
                        //});

                        await consumer.AcknowledgeAsync(message.MessageId);

                        await producer.DisposeAsync();
                    },
                    maxDegreeOfParallelism: 16,
                    cts.Token);

                ////pump.Stop();

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
}