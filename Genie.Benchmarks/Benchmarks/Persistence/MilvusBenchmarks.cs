using Genie.Adapters.Persistence.Milvus;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class MilvusBenchmarks : PersistenceBase
{
    public MilvusBenchmarks()
    {
        persistenceTest = new MilvusTest(payload, new DefaultObjectPool<MilvusPooledObject>(new DefaultPooledObjectPolicy<MilvusPooledObject>()),
            new DefaultObjectPool<MilvusPooledObject2>(new DefaultPooledObjectPolicy<MilvusPooledObject2>())
            );
    }
}