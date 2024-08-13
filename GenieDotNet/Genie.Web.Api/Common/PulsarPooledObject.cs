using Genie.Common;
using Genie.Common.Web;
using Pulsar.Client.Api;

namespace Genie.Web.Api.Common;
public class PulsarPooledObject : GeniePooledObject
{
    public PulsarPooledObject() : base()
    {

    }

    public void Configure(GenieContext genieContext)
    {
        PulsarClient = new PulsarClientBuilder()
            .ServiceUrl("pulsar://pulsar:6650")
            .BuildAsync().GetAwaiter().GetResult();

        Producer = PulsarClient.NewProducer()
            .Topic(genieContext.Kafka.Ingress)
            .CreateAsync().GetAwaiter().GetResult();

        Consumer = PulsarClient.NewConsumer()
            //.NewConsumer<EventTaskJob>(Schema.AVRO<EventTaskJob>())
            .Topic(EventChannel)
            .SubscriptionName("subscriptionName")
            .SubscribeAsync().GetAwaiter().GetResult();
    }

    public PulsarClient? PulsarClient { get; set; }

    public IProducer<byte[]>? Producer { get; set; }
    public IConsumer<byte[]>? Consumer { get; set; }

}