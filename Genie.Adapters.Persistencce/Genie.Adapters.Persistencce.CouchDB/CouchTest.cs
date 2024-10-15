using Confluent.Kafka;
using CouchDB.Driver.Types;
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.Couch;

public class CouchPersistenceTest : CouchDocument
{
    public string Info { get; set; }
}


public class CouchTest(int payload, ObjectPool<CouchPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    public ObjectPool<CouchPooledObject> Pool = pool;

    public CouchTest() : this(JsonSize,new DefaultObjectPool<CouchPooledObject>(new DefaultPooledObjectPolicy<CouchPooledObject>()))
    {

    }

    public CouchTest(bool create) : this(JsonSize,new DefaultObjectPool<CouchPooledObject>(new DefaultPooledObjectPolicy<CouchPooledObject>()))
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
        CouchPooledObject.Postal.CreateIndexAsync("idx_postal", b => b.IndexBy(a => a.PostalCode));
    }


    public override bool WriteJson(long i)
    {
        var success = true;

        try
        {
            //var result = CouchPooledObject.Json.FindAsync(i.ToString()).GetAwaiter().GetResult();

            //result ??= new CouchPersistenceTest
            //    {
            //        Id = i.ToString(),
            //        Info = new('-', Payload)
            //    };

            CouchPooledObject.Json.AddOrUpdateAsync(new CouchPersistenceTest
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            }).GetAwaiter().GetResult();
        }
        catch(Exception ex)
        {
            success = false;
        }


        return success;
    }

    public override bool ReadJson(long i)
    {
        bool result = true;

        try
        {
            var match = CouchPooledObject.Json.FindAsync(i.ToString()).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }


    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        
        try
        {
            await CouchPooledObject.Postal.AddAsync(new CountryPostalCodeCouch
            {
                Id = message.Id.ToString(),
                CountryCode = message.CountryCode,
                PostalCode = message.PostalCode,
                PlaceName = message.PlaceName,
                Latitude = message.Latitude,
                Longitude = message.Longitude
            });
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }

    public override async Task<bool> UpdatePostal(CountryPostalCode message)
    {
        bool result = true;

        try
        {
            var update = await CouchPooledObject.Postal.FindAsync(message.Id.ToString());
            update.PlaceName = message.Id.ToString();

            await CouchPooledObject.Postal.AddOrUpdateAsync(update, new CouchDB.Driver.DatabaseApiMethodOptions.AddOrUpdateOptions
            { Batch = true, Rev = update.Rev });
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }


    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;

        try
        {
            var match = await CouchPooledObject.Postal.FindAsync(message.Id.ToString());
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }

    public override async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;

        try
        {
            var results = await CouchPooledObject.Postal.QueryAsync($$"""
                {
                    "selector": {
                        "postalCode": { "$eq": "{{message.PostalCode}}" }
                    }
                }
                """);
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;


        try
        {
            var match = await CouchPooledObject.Postal.FindAsync(message.Id.ToString());
            //var cc = match?.As<CountryPostalCodeCouch>();

            var results = await CouchPooledObject.Postal.QueryAsync($$"""
                {
                    "selector": {
                        "postalCode": { "$eq": "{{match.PostalCode}}" }
                    }
                }
                """);
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;

    }
}