
using Genie.Adapters.Persistence.Neo4j;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class Neo4jBenchmarks : PersistenceBase
{
    public Neo4jBenchmarks() {
        persistenceTest = new Neo4jTest(payload, new DefaultObjectPool<Neo4jPooledObject>(new DefaultPooledObjectPolicy<Neo4jPooledObject>()));
    }
}