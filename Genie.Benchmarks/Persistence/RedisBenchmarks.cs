
using Genie.Adapters.Persistence.Redis;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class RedisBenchmarks : PersistenceBase
{
    public RedisBenchmarks() {
        persistenceTest = new RedisTest(payload, new DefaultObjectPool<RedisPooledObject>(new DefaultPooledObjectPolicy<RedisPooledObject>()));
    }
}