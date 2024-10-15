using Genie.Adapters.Persistence.Couch;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class CouchBenchmarks : PersistenceBase
{
    public CouchBenchmarks()
    {
        persistenceTest = new CouchTest(payload, new DefaultObjectPool<CouchPooledObject>(new DefaultPooledObjectPolicy<CouchPooledObject>()));
    }
}