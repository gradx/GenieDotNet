using Genie.Adapters.Persistence.CrateDB;
using Microsoft.Extensions.ObjectPool;


namespace Genie.Benchmarks.Benchmarks.Persistence;

public class CrateBenchmarks : PersistenceBase
{
    public CrateBenchmarks() {
        persistenceTest = new CrateTest(payload, new DefaultObjectPool<CratePooledObject>(new DefaultPooledObjectPolicy<CratePooledObject>()));
    }
}