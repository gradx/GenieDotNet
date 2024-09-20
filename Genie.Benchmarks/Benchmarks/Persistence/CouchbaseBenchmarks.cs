using Genie.Adapters.Persistence.Couchbase;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class CouchbaseBenchmarks : PersistenceBase
{
    public CouchbaseBenchmarks()
    {
        persistenceTest = new CouchbaseTest(payload, new DefaultObjectPool<CouchbasePooledObject>(new DefaultPooledObjectPolicy<CouchbasePooledObject>()));
    }
}