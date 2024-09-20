using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Redis;

public class RedisTest(int payload, ObjectPool<RedisPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<RedisPooledObject> Pool = pool;

    public void Write(int i)
    {
        string id = $@"new{i}";
        var test = new PersistenceTest
        {
            Id = id,
            Info = new('-', Payload)
        };

        var lease = Pool.Get();

        var result = lease.Database.StringSet(id, JsonSerializer.Serialize(test));

        Pool.Return(lease);
    }

    public void Read(int i)
    {
        string id = $@"new{i}";

        var lease = Pool.Get();
        var json = lease.Database.StringGet(id);
        var result = JsonSerializer.Deserialize<PersistenceTest>(json!);


        Pool.Return(lease);
    }
}