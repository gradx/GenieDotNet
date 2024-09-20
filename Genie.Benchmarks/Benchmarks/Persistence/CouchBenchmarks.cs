using Genie.Adapters.Persistence.CouchDB;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class CouchBenchmarks : PersistenceBase
{
    public CouchBenchmarks()
    {
        persistenceTest = new CouchTest(payload, new DefaultObjectPool<CouchPooledObject>(new DefaultPooledObjectPolicy<CouchPooledObject>()));
    }
}