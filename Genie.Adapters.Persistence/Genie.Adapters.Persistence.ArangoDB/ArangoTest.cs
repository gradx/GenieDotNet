using ArangoDBNetStandard.DatabaseApi.Models;
using ArangoDBNetStandard.DatabaseApi;
using ArangoDBNetStandard.Transport.Http;
using Genie.Utils;
using ArangoDBNetStandard.CollectionApi.Models;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.ArangoDB;

public class ArangoTest(int payload, ObjectPool<ArangoPooledObject> pool) : IPersistenceTest
{

    public int Payload { get; set; } = payload;
    public ObjectPool<ArangoPooledObject> Pool => pool;


    public static async Task CreateDB()
    {
        // You must use the _system database to create databases
        using (var systemDbTransport = HttpApiTransport.UsingBasicAuth(
            new Uri("http://localhost:8529/"),
            "_system",
            "root",
            "PFHiVVLs4NcgP6GA"))
        {
            var systemDb = new DatabaseApiClient(systemDbTransport);

            // Create a new database with one user.
            await systemDb.PostDatabaseAsync(
                new PostDatabaseBody
                {
                    Name = "genie",
                    Users =
                    [
                        new() {
                            Username = "admin",
                            Passwd = "pass"
                        }
                    ]
                });
        }

        var test = new ArangoTest(4000, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()));

        var lease = test.Pool.Get();

        // Create a collection in the database
        await lease.Client.Collection.PostCollectionAsync(
            new PostCollectionBody
            {
                Name = "Benchmarks"
                // A whole heap of other options exist to define key options, 
                // sharding options, etc
            });

        test.Pool.Return(lease);
    }

    public void Write(int i)
    {
        var test = new PersistenceTest
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();
        var result = lease.Client.Document.PostDocumentAsync("Benchmarks", test).GetAwaiter().GetResult();

        Pool.Return(lease);
    }

    public void Read(int i)
    {
        var lease = Pool.Get();
        var response = lease.Client.Cursor.PostCursorAsync<PersistenceTest>(
            $@"FOR doc IN Benchmarks 
              FILTER doc.Id == 'new{i}'
              RETURN doc").GetAwaiter().GetResult();

        var item = response.Result.First();

        Pool.Return(lease);
    }
}