using ArangoDBNetStandard.DatabaseApi.Models;
using ArangoDBNetStandard.DatabaseApi;
using ArangoDBNetStandard.Transport.Http;
using Genie.Utils;
using ArangoDBNetStandard.CollectionApi.Models;
using Microsoft.Extensions.ObjectPool;
using static System.Net.Mime.MediaTypeNames;

namespace Genie.Adapters.Persistence.ArangoDB;

public class ArangoTest(int payload, ObjectPool<ArangoPooledObject> pool) : IPersistenceTest
{

    public int Payload { get; set; } = payload;
    public ObjectPool<ArangoPooledObject> Pool => pool;


    public static async Task CreateDB(string name)
    {
        // You must use the _system database to create databases
        //using (var systemDbTransport = HttpApiTransport.UsingBasicAuth(
        //    new Uri("http://localhost:8529/"),
        //    "_system",
        //    "root",
        //    "q8klm9xR8ctRetsW"))
        //{
        //    var systemDb = new DatabaseApiClient(systemDbTransport);

        //    // Create a new database with one user.
        //    await systemDb.PostDatabaseAsync(
        //        new PostDatabaseBody
        //        {
        //            Name = "genie",
        //            Users =
        //            [
        //                new() {
        //                    Username = "admin",
        //                    Passwd = "pass"
        //                }
        //            ]
        //        });
        //}

        var test = new ArangoTest(4000, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()));

        var lease = test.Pool.Get();

        // Create a collection in the database
        await lease.Client.Collection.PostCollectionAsync(
            new PostCollectionBody
            {
                Name = name
                // A whole heap of other options exist to define key options, 
                // sharding options, etc
            });

        test.Pool.Return(lease);
    }

    public async Task<bool> CreateIndex(string collectionName, string field, bool unique)
    {
        var success = true;
        var lease = Pool.Get();

        try
        {
            await lease.Client.Index.PostPersistentIndexAsync(new ArangoDBNetStandard.IndexApi.Models.PostIndexQuery { CollectionName = collectionName },
                new ArangoDBNetStandard.IndexApi.Models.PostPersistentIndexBody { Fields = [field], Unique = unique });
        }
        catch (Exception ex)
        {
            success = false;
        }


        Pool.Return(lease);
        return success;

    }

    public bool Write(long i)
    {
        bool success = true;

        var test = new PersistenceTest
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();
        try
        {
            var result = lease.Client.Document.PostDocumentAsync("Benchmarks", test).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            success = false;
        }
        

        Pool.Return(lease);
        return success;
    }

    public bool Read(long i)
    {
        var success = true;
        var lease = Pool.Get();

        try
        {
            var response = lease.Client.Cursor.PostCursorAsync<PersistenceTest>(
                $@"FOR doc IN Benchmarks 
              FILTER doc.Id == 'new{i}'
              RETURN doc").GetAwaiter().GetResult();

            var item = response.Result.First();
        }
        catch (Exception ex)
        {
            success = false;
        }


        Pool.Return(lease);
        return success;
    }

    public async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
             await lease.Client.Document.PostDocumentAsync("CountryCodes", message);
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            var response = await lease.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN CountryCodes 
              FILTER doc.Id == {message.Id}
              RETURN doc");

            var item = response.Result.First();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
    public async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {

            var response = await lease.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN CountryCodes 
              FILTER doc.PostalCode == '{message.PostalCode}'
              RETURN doc");

            var item = response.Result.First();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            var id_response = await lease.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN CountryCodes 
              FILTER doc.Id == {message.Id}
              RETURN doc");

            var item = id_response.Result.First();

            var postal_codes = await lease.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN CountryCodes 
              FILTER doc.PostalCode == '{item.PostalCode}'
              RETURN doc");

        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}