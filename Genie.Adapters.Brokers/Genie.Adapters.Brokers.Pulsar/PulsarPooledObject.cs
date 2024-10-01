using Genie.Common;
using Genie.Utils;
using Pulsar.Client.Api;

namespace Genie.Adapters.Brokers.Pulsar;
public class PulsarPooledObject : GeniePooledObject
{
    public PulsarPooledObject() : base()
    {

    }

    public void Configure(GenieContext genieContext)
    {
        PulsarClient = new PulsarClientBuilder()
            .ServiceUrl(genieContext.Pulsar.ConnectionString)
            .BuildAsync().GetAwaiter().GetResult();

        Producer = PulsarClient.NewProducer()
            .Topic(genieContext.Kafka.Ingress)
            .CreateAsync().GetAwaiter().GetResult();

        Consumer = PulsarClient.NewConsumer()
            .Topic(EventChannel)
            .SubscriptionName(this.GetType().Name)
            .SubscribeAsync().GetAwaiter().GetResult();
    }

    public PulsarClient? PulsarClient { get; set; }

    public IProducer<byte[]>? Producer { get; set; }
    public IConsumer<byte[]>? Consumer { get; set; }

}