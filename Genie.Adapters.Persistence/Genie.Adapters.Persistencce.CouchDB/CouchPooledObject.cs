using CouchDB.Driver;

namespace Genie.Adapters.Persistence.CouchDB;

public class CouchPooledObject
{
    public static readonly CouchClient Client;
    public static readonly ICouchDatabase<CouchPersistenceTest> Database;

    static CouchPooledObject()
    {

        Client = new CouchClient("http://localhost:5984/", builder => builder
            .UseEndpoint("http://localhost:5984/")
            .UseBasicAuthentication("admin", "password"));


        Database = Client.GetOrCreateDatabaseAsync<CouchPersistenceTest>().GetAwaiter().GetResult();
    }
}
