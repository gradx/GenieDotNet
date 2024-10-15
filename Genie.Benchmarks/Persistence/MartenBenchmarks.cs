
using Genie.Adapters.Persistence.Marten;
using Microsoft.Extensions.ObjectPool;


namespace Genie.Benchmarks.Benchmarks.Persistence;

public class MartenBenchmarks : PersistenceBase
{
    public MartenBenchmarks() {
        persistenceTest = new MartenTest(payload, new DefaultObjectPool<MartenPooledObject>(new DefaultPooledObjectPolicy<MartenPooledObject>()));
    }
}