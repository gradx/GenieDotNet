
namespace Genie.Core.Settings;
public class RabbitMQSettings(string exchange, string queue, string routingKey, string user, string pass, string vhost, string host)
{
    public string Exchange => exchange;
    public string Queue => queue;
    public string RoutingKey => routingKey;
    public string Host => host;
    public string User => user;
    public string Pass => pass;
    public string Vhost => vhost;
}