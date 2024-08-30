using MQTTnet;
using MQTTnet.Client;

namespace Genie.Scratch.MQTTNet
{
    public class MQTTTest
    {
        public static async Task Start()
        {
            var mqttFactory = new MqttFactory();
            var mqttClient = mqttFactory.CreateMqttClient();


            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883) // Port is optional
                .Build();

            await mqttClient.ConnectAsync(options, CancellationToken.None);

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Received application message.");

                return Task.CompletedTask;
            };

            var topic = new MqttTopicFilterBuilder().WithTopic("MyTopic").Build();
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(topic).Build();
            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("MyTopic")
                .WithPayload("Hello World")
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(message, CancellationToken.None); // Since 3.0.5 with CancellationToken


            Thread.Sleep(60000);
        }
    }
}
