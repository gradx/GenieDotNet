
using Chr.Avro.Abstract;
using Chr.Avro.Confluent;
using Chr.Avro.Serialization;
using Confluent.Kafka;
using Genie.Common;
using Genie.Common.Adapters.Kafka;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Web.Api.Mediator.Commands;

namespace Genie.Web.Api.Common;
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