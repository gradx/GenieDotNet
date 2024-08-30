using NATS.Client.Core;


namespace Genie.Scratch.NATS
{
    public class NatsExample
    {
        public static async Task StartSubscriber()
        {
            //var opts = new NatsOpts() { Url = "http://localhost:4222" };
            await using var nats = new NatsConnection();

            // Subscribe on one terminal
            await foreach (var msg in nats.SubscribeAsync<string>(subject: "foo"))
            {
                Console.WriteLine($"Received: {msg.Data}");
            }

        }

        public static async Task StartPublisher()
        {
            await using var nats = new NatsConnection();


            // Start publishing to the same subject on a second terminal
            await nats.PublishAsync(subject: "foo", data: "Hello, World!");
        }
    }
}
