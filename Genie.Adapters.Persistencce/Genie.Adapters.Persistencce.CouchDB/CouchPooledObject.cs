using CouchDB.Driver;
using Genie.Core.Persistence;
using Microsoft.Azure.Cosmos;

namespace Genie.Adapters.Persistence.Couch;

public class CouchPooledObject
{
    public static readonly CouchClient Client;
    public static readonly ICouchDatabase<CouchPersistenceTest> Json;
    public static readonly ICouchDatabase<CountryPostalCodeCouch> Postal;

    static CouchPooledObject()
    {

        Client = new CouchClient("http://localhost:5984/", builder => builder
            .UseEndpoint("http://localhost:5984/")
            .UseBasicAuthentication("admin", "password"));


        Json = Client.GetOrCreateDatabaseAsync<CouchPersistenceTest>().GetAwaiter().GetResult();
        Postal = Client.GetOrCreateDatabaseAsync<CountryPostalCodeCouch>().GetAwaiter().GetResult();
    }
}