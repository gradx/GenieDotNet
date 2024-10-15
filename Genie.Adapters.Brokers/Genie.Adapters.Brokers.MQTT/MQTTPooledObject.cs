using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Adapters.Serializers.Avro;
using Genie.Core;
using MQTTnet;
using MQTTnet.Client;

namespace Genie.Adapters.Brokers.MQTT;

public class MQTTPooledObject<T> : GeniePooledObject
{
    public T? Result { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);
    private IMqttClient? MQTTClient { get; set; }
    private BinaryDeserializer<T>? Deserializer { get; set; }

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



        var schema = schemaBuilder.BuildSchema<T>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<T>(schema);
    }

    public void Send(byte[] data)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("Genie")
            .WithPayload(data)
            .Build();

        MQTTClient?.PublishAsync(message, CancellationToken.None).GetAwaiter().GetResult();
    }

    public T Deserialize(byte[] data)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(data);
        return Deserializer!(ref reader);
    }
}