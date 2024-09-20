
using Genie.Adapters.Persistence.CockroachDB;
using Microsoft.Extensions.ObjectPool;


namespace Genie.Benchmarks.Benchmarks.Persistence;

public class CockroackBenchmarks : PersistenceBase
{
    public CockroackBenchmarks() {
        persistenceTest = new CockroachTest(this.payload, new DefaultObjectPool<CockroackPooledObject>(new DefaultPooledObjectPolicy<CockroackPooledObject>()));
    }
}