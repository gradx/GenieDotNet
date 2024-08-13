using Genie.Common.Settings;
using RabbitMQ.Client;

namespace Genie.Common.Adapters.RabbitMQ;

public class RabbitUtils
{
    public static IConnection GetConnection(RabbitSettings settings, bool isAsync)
    {
        ConnectionFactory factory = new ConnectionFactory();

        factory.Uri = new Uri($@"amqp://{settings.User}:{settings.Pass}@{settings.Host}:5672/");
        factory.DispatchConsumersAsync = isAsync;
        return factory.CreateConnection();
    }
}