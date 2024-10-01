using Genie.Adapters.Persistence.MongoDB;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class MongoBenchmarks : PersistenceBase
{
    public MongoBenchmarks()
    {
        persistenceTest = new MongoTest(payload, 
            new DefaultObjectPool<MongoPooledObject<PersistenceTestModel>>(new DefaultPooledObjectPolicy<MongoPooledObject<PersistenceTestModel>>()),
            new DefaultObjectPool<MongoPooledObject<CountryPostalCode>>(new DefaultPooledObjectPolicy<MongoPooledObject<CountryPostalCode>>())
            );
    }
}