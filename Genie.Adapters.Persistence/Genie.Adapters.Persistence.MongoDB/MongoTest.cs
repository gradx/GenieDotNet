using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using MongoDB.Driver;

namespace Genie.Adapters.Persistence.MongoDB;

public class MongoTest(int payload, ObjectPool<MongoPooledObject<PersistenceTest>> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MongoPooledObject<PersistenceTest>> Pool = pool;

    public bool Write(long i)
    {
        bool success = true;
        var test = new PersistenceTest
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();

        if (lease.Collection == null)
            lease.Configure("PersistenceTest");

        var result = lease.Collection!.ReplaceOne(Builders<PersistenceTest>.Filter.Eq(r => r.Id, $@"new{i}"), test, new ReplaceOptions { IsUpsert = true });

        Pool.Return(lease);
        return success;
    }


    public bool Read(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            if (lease.Collection == null)
                lease.Configure("PersistenceTest");

            var results = lease.Collection.Find(Builders<PersistenceTest>.Filter.Eq(r => r.Id, $@"new{i}")).ToList();

            Pool.Return(lease);
        }
        catch (Exception ex)
        {
            success = false;
        }

        return success;
    }

    public async Task<bool> WritePostal(CountryPostalCode message)
    {
        return true;
    }

    public async Task<bool> ReadPostal(CountryPostalCode message)
    {
        return true;
    }

    public async Task<bool> QueryPostal(CountryPostalCode message)
    {
        return true;

    }

    public async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        return true;
    }
}