using Genie.Core.Persistence;
using Humanizer.Localisation;
using Microsoft.Extensions.ObjectPool;
using MongoDB.Driver;
using System.Runtime.InteropServices.Marshalling;

namespace Genie.Adapters.Persistence.MongoDB;

public class MongoTest(int payload, ObjectPool<MongoPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    public MongoTest() : this(JsonSize,new DefaultObjectPool<MongoPooledObject>(new DefaultPooledObjectPolicy<MongoPooledObject>()))
    {

    }

    public MongoTest(bool create) : this(JsonSize,new DefaultObjectPool<MongoPooledObject>(new DefaultPooledObjectPolicy<MongoPooledObject>()))
    {
        if (create)
            CreateDB();
    }


    public override void CreateDB()
    {
        CreateIndex();
    }

    public override void CreateIndex()
    {
        var lease = pool.Get();

        var indexModel = new CreateIndexModel<CountryPostalCode>(Builders<CountryPostalCode>.IndexKeys.Ascending(m => m.PostalCode));
        lease.Postal.Indexes.CreateOne(indexModel);

        pool.Return(lease);
    }


    public override bool WriteJson(long i)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var test = new PersistenceTestModel
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            };

            var result = lease.Json!.ReplaceOne(Builders<PersistenceTestModel>.Filter.Eq(r => r.Id, i.ToString()), test, new ReplaceOptions { IsUpsert = true });
        }
        catch (Exception ex)
        {
            success = false;
        }

        pool.Return(lease);
        return success;
    }

    public override bool ReadJson(long i)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var query = lease.Json.Find(Builders<PersistenceTestModel>.Filter.Eq(r => r.Id, i.ToString()));
            var matches = query.FirstOrDefault();
        }
        catch (Exception ex)
        {
            success = false;
        }

        pool.Return(lease);
        return success;
    }

    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var result = lease.Postal.ReplaceOne(Builders<CountryPostalCode>.Filter.Eq(r => r.Id, message.Id),
                message,
                new ReplaceOptions { IsUpsert = true });
        }
        catch(Exception ex)
        {
            success = false;
        }

        pool.Return(lease);
        return success;
    }

    public override async Task<bool> UpdatePostal(CountryPostalCode message)
    {
        return await WritePostal(message);
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var query = lease.Postal!.Find(Builders<CountryPostalCode>.Filter.Eq(r => r.Id, message.Id));
            var match = query.FirstOrDefault();
        }
        catch (Exception ex)
        {
            success = false;
        }

        pool.Return(lease);
        return success;
    }

    public override async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var query = lease.Postal!.Find(Builders<CountryPostalCode>.Filter.Eq(r => r.PostalCode, message.PostalCode));
            var matches = query.ToList();
        }
        catch(Exception ex)
        {
            success = false;
        }

        pool.Return(lease);
        return success;
    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var query = lease.Postal!.Find(Builders<CountryPostalCode>.Filter.Eq(r => r.Id, message.Id));
            var read = query.FirstOrDefault();
            var joined = lease.Postal!.Find(Builders<CountryPostalCode>.Filter.Eq(r => r.PostalCode, read.PostalCode));
            var matches = joined.ToList();
        }
        catch(Exception ex)
        {
            success = true;
        }

        pool.Return(lease);
        return success;
    }
}