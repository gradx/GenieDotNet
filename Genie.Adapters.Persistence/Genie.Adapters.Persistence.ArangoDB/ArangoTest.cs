
using ArangoDBNetStandard.CollectionApi.Models;
using ArangoDBNetStandard.Transport.Http;
using ArangoDBNetStandard.DatabaseApi.Models;
using ArangoDBNetStandard.DatabaseApi;
using Microsoft.Extensions.ObjectPool;
using Genie.Core.Persistence;
using System.ComponentModel;
using Confluent.Kafka;

namespace Genie.Adapters.Persistence.ArangoDB;

public class CountryPostalCodeArango : CountryPostalCode
{
    public string _key { get; set; }

    public static CountryPostalCodeArango Get(CountryPostalCode message)
    {
        return new CountryPostalCodeArango
        {
            Id = message.Id,
            CountryCode = message.CountryCode,
            PostalCode = message.PostalCode,
            PlaceName = message.PlaceName,
            Latitude = message.Latitude,
            Longitude = message.Longitude,
            _key = message.Id.ToString()
        };
    }
}

public class ArangoTest(int payload, ObjectPool<ArangoPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    public ObjectPool<ArangoPooledObject> Pool => pool;

    private const string Password = "8yCFgSYpVlWF6v8m";

    public ArangoTest() : this(JsonSize, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()))
    {

    }

    public ArangoTest(bool create) : this(JsonSize, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()))
    {
        if (create)
            CreateDB();
    }

    public override void CreateDB()
    {
        CreateDB("genie");
        
        CreateCollection("json_data");
        CreateIndex("json_data", "Id", true).GetAwaiter().GetResult();

        CreateCollection("country_data");

        CreateIndex();
    }

    public override void CreateIndex()
    {
        CreateIndex("country_data", "Id", true).GetAwaiter().GetResult();
        CreateIndex("country_data", "PostalCode", false).GetAwaiter().GetResult();
    }

    public void CreateJsonDB()
    {
        CreateDB("genie");

        CreateCollection("json_data");
        CreateIndex("json_data", "Id", true).GetAwaiter().GetResult();
    }

    public void CreatePostalDB()
    {
        CreateDB("genie");

        CreateCollection("country_data");
    }

    private void CreateDB(string name)
    {
        // You must use the _system database to create databases
        using (var systemDbTransport = HttpApiTransport.UsingBasicAuth(
            new Uri("http://localhost:8529/"),
            "_system",
            "root",
            Password))
        {
            var systemDb = new DatabaseApiClient(systemDbTransport);

            // Create a new database with one user.
            systemDb.PostDatabaseAsync(
                new PostDatabaseBody
                {
                    Name = name,
                    Users =
                    [
                        new() {
                            Username = "admin",
                            Passwd = "pass"
                        }
                    ]
                }).GetAwaiter().GetResult();
        }
    }

    private void CreateCollection(string name)
    {
        var test = new ArangoTest(4000, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()));

        ArangoPooledObject.Client.Collection.PostCollectionAsync(
            new PostCollectionBody
            {
                Name = name
            }).GetAwaiter().GetResult();
    }

    private async Task<bool> CreateIndex(string collectionName, string field, bool unique)
    {
        var success = true;

        try
        {
            await ArangoPooledObject.Client.Index.PostPersistentIndexAsync(new ArangoDBNetStandard.IndexApi.Models.PostIndexQuery { CollectionName = collectionName },
                new ArangoDBNetStandard.IndexApi.Models.PostPersistentIndexBody { Fields = [field], Unique = unique });
        }
        catch (Exception ex)
        {
            success = false;
        }

        return success;
    }

    public override bool WriteJson(long i)
    {
        bool success = true;

        try
        {
            var test = new PersistenceTestModel
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            };

            var result = ArangoPooledObject.Client.Document.PostDocumentAsync("json_data", test).GetAwaiter().GetResult();
        }
        catch (Exception ex)
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
            var response = ArangoPooledObject.Client.Cursor.PostCursorAsync<PersistenceTestModel>(
                $@"FOR doc IN json_data 
              FILTER doc.Id == '{i}'
              RETURN doc").GetAwaiter().GetResult();

            var item = response.Result.First();
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
             var help = await ArangoPooledObject.Client.Document.PostDocumentAsync("country_data", CountryPostalCodeArango.Get(message));
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
            await ArangoPooledObject.Client.Document.PutDocumentAsync("country_data", message.Id.ToString(), CountryPostalCodeArango.Get(message));
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
            
            var response = await ArangoPooledObject.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN country_data 
                      FILTER doc.Id == {message.Id}
                      RETURN doc");

            var item = response.Result.First();
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
            var response = await ArangoPooledObject.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN country_data 
              FILTER doc.PostalCode == '{message.PostalCode}'
              RETURN doc");

            var matches = response.Result.ToList();
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
            var id_response = await ArangoPooledObject.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN country_data 
              FILTER doc.Id == {message.Id}
              RETURN doc");

            var item = id_response.Result.First();

            var postal_codes = await ArangoPooledObject.Client.Cursor.PostCursorAsync<CountryPostalCode>(
                $@"FOR doc IN country_data 
              FILTER doc.PostalCode == '{item.PostalCode}'
              RETURN doc");

        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }
}