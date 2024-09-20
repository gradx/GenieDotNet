using Genie.Adapters.Persistence.MariaDB;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class MariaBenchmarks : PersistenceBase
{
    public MariaBenchmarks()
    {
        persistenceTest = new MariaTest(payload, new DefaultObjectPool<MariaPooledObject>(new DefaultPooledObjectPolicy<MariaPooledObject>()));
    }
}