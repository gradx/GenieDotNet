using Genie.Common.Settings;
using RabbitMQ.Client;

namespace Genie.Common.Adapters.RabbitMQ;

public sealed class RabbitUtils
{
    private static readonly Lazy<IConnection> lazy = new(() => GetConnection());

    public static IConnection Instance { get { return lazy.Value; } }

    private static IConnection GetConnection()
    {
        var context = GenieContext.Build().GenieContext;
        ConnectionFactory factory = new ConnectionFactory();

        factory.Uri = new Uri($@"amqp://{context.RabbitMQ.User}:{context.RabbitMQ.Pass}@{context.RabbitMQ.Host}:5672/");
        //factory.ConsumerDispatchConcurrency = 1;
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }
}