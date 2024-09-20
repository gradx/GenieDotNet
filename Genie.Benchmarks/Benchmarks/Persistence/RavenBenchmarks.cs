using Genie.Adapters.Persistence.RavenDB;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;
public class RavenBenchmarks : PersistenceBase
{
    public RavenBenchmarks()
    {
        persistenceTest = new RavenTest(payload, new DefaultObjectPool<RavenPooledObject>(new DefaultPooledObjectPolicy<RavenPooledObject>()));
    }
}