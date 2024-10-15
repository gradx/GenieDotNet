
using Genie.Adapters.Persistence.Aerospike;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class AerospikeBenchmarks : PersistenceBase
{
    public AerospikeBenchmarks() {
        persistenceTest = new AerospikeTest(payload, new DefaultObjectPool<AerospikePooledObject>(new DefaultPooledObjectPolicy<AerospikePooledObject>()));
    }
}