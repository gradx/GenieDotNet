
using Genie.Adapters.Persistence.Redis;
using Genie.Adapters.Persistence.Scylla;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class ScyllaBenchmarks : PersistenceBase
{
    public ScyllaBenchmarks() {
        persistenceTest = new ScyllaTest(payload, new DefaultObjectPool<ScyllaPooledObject>(new DefaultPooledObjectPolicy<ScyllaPooledObject>()));
    }
}