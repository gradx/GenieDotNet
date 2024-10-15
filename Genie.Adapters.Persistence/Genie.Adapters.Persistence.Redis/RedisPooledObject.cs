using StackExchange.Redis;

namespace Genie.Adapters.Persistence.Redis;

public class RedisPooledObject
{
    public readonly ConnectionMultiplexer Client;
    public readonly IDatabase Database;

    public RedisPooledObject()
    {
        Client = ConnectionMultiplexer.Connect("localhost");
        Database = Client.GetDatabase();
    }
}