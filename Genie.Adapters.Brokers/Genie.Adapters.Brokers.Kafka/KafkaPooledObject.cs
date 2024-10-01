
using Chr.Avro.Confluent;
using Confluent.Kafka;
using Genie.Common;
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Utils;

namespace Genie.Adapters.Brokers.Kafka;

public class KafkaPooledObject : GeniePooledObject
{
    public KafkaPooledObject() : base()
    {

    }

    public IConsumer<string, EventTaskJob>? Consumer { get; set; }

    public async Task Configure(GenieContext genieContext, KafkaCommand command)
    {
        var builder = new ConsumerBuilder<string, EventTaskJob>(KafkaUtils.GetConfig(genieContext));

        builder.SetAvroKeyDeserializer(command.SchemaRegistry);
        builder.SetAvroValueDeserializer(command.SchemaRegistry);

        Consumer = builder.Build();

        await KafkaUtils.CreateTopic(command.AdminClient, [this.EventChannel]);
        Consumer.Subscribe(EventChannel);
    }
}