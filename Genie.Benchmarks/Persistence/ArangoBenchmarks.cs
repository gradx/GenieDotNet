
using Genie.Adapters.Persistence.ArangoDB;
using Microsoft.Extensions.ObjectPool;


namespace Genie.Benchmarks.Benchmarks.Persistence;

public class ArangoBenchmarks : PersistenceBase
{
    public ArangoBenchmarks() {
        persistenceTest = new ArangoTest(payload, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()));
    }
}