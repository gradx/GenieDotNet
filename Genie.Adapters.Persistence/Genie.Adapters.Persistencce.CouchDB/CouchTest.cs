using CouchDB.Driver;
using CouchDB.Driver.Options;
using CouchDB.Driver.Types;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.CouchDB;

public class CouchPersistenceTest : CouchDocument
{
    public string Info { get; set; }
}


public class CouchTest(int payload, ObjectPool<CouchPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;

    public ObjectPool<CouchPooledObject> Pool = pool;


    public bool Write(long i)
    {
        var success = true;
        var lease = Pool.Get();
        var result = CouchPooledObject.Database.FindAsync($@"new{i}").GetAwaiter().GetResult();

        result ??= new CouchPersistenceTest
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };

        CouchPooledObject.Database.AddOrUpdateAsync(result).GetAwaiter().GetResult();

        Pool.Return(lease);
        return success;
    }

    public bool Read(long i)
    {
        var success = true;


        var result = CouchPooledObject.Database.FindAsync($@"new{i}").GetAwaiter().GetResult();

        return success;
    }

    public async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        
        try
        {
            await CouchPooledObjectCountry.Database.AddAsync(new CountryPostalCodeCouch
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

    public async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;

        try
        {
            var match = await CouchPooledObjectCountry.Database.FindAsync($@"{message.Id}");
            //var cc = match?.As<CountryPostalCodeCouch>();
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }

    public async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;

        try
        {
            var results = await CouchPooledObjectCountry.Database.QueryAsync($$"""
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

    public async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;


        try
        {
            var match = await CouchPooledObjectCountry.Database.FindAsync($@"{message.Id}");
            //var cc = match?.As<CountryPostalCodeCouch>();

            var results = await CouchPooledObjectCountry.Database.QueryAsync($$"""
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