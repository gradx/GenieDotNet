using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using MQTTnet;
using MQTTnet.Client;

namespace Genie.Adapters.Brokers.MQTT;

public class MQTTPooledObject : GeniePooledObject
{
    public EventTaskJob? Result { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);
    private IMqttClient? MQTTClient { get; set; }
    private BinaryDeserializer<EventTaskJob>? Deserializer { get; set; }

    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {
        var mqttFactory = new MqttFactory();
        MQTTClient = mqttFactory.CreateMqttClient();


        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883) // Port is optional
            .Build();


        MQTTClient.ConnectAsync(options, CancellationToken.None).GetAwaiter().GetResult();

        MQTTClient.ApplicationMessageReceivedAsync += e =>
        {
            Result = Deserialize(e.ApplicationMessage.PayloadSegment.Array!);
            ReceiveSignal.Set();

            return Task.CompletedTask;
        };

        var topic = new MqttTopicFilterBuilder().WithTopic(EventChannel).Build();
        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(topic).Build();

        var sub = MQTTClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None).GetAwaiter().GetResult();



        var schema = schemaBuilder.BuildSchema<EventTaskJob>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<EventTaskJob>(schema);
    }

    public void Send(byte[] data)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("Genie")
            .WithPayload(data)
            .Build();

        MQTTClient?.PublishAsync(message, CancellationToken.None).GetAwaiter().GetResult();
    }

    public EventTaskJob Deserialize(byte[] data)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(data);
        return Deserializer!(ref reader);
    }
}