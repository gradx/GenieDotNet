
using Chr.Avro.Confluent;
using Confluent.Kafka;
using Genie.Common.Types;
using Genie.Core;
using Genie.Web.Api.Commands;
using NetTopologySuite.IO;

namespace Genie.Adapters.Brokers.Kafka;

public class KafkaPooledObject<T> : GeniePooledObject
{
    public KafkaPooledObject() : base()
    {

    }

    public IConsumer<string, T>? Consumer { get; set; }

    public async Task Configure(GenieContext genieContext, KafkaCommand command)
    {
        var builder = new ConsumerBuilder<string, T>(KafkaUtils.GetConfig(genieContext));

        builder.SetAvroKeyDeserializer(command.SchemaRegistry);
        builder.SetAvroValueDeserializer(command.SchemaRegistry);

        Consumer = builder.Build();

        await KafkaUtils.CreateTopic(command.AdminClient, [this.EventChannel]);
        Consumer.Subscribe(EventChannel);
    }
}