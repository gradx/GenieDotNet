using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using MongoDB.Driver;

namespace Genie.Adapters.Persistence.MongoDB;

public class MongoTest(int payload, ObjectPool<MongoPooledObject<PersistenceTestModel>> pool, ObjectPool<MongoPooledObject<CountryPostalCode>> pool2) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MongoPooledObject<PersistenceTestModel>> Pool = pool;
    readonly ObjectPool<MongoPooledObject<CountryPostalCode>> Pool2 = pool2;

    public override bool WriteJson(long i)
    {
        bool success = true;
        var test = new PersistenceTestModel
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();

        if (lease.Collection == null)
            lease.Configure("PersistenceTest");

        var result = lease.Collection!.ReplaceOne(Builders<PersistenceTestModel>.Filter.Eq(r => r.Id, $@"new{i}"), test, new ReplaceOptions { IsUpsert = true });

        Pool.Return(lease);
        return success;
    }

    public override bool ReadJson(long i)
    {
        return true;
    }

    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        return true;
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        return true;
    }

    public override async Task<bool> QueryPostal(CountryPostalCode message)
    {
        return true;

    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        return true;
    }
}