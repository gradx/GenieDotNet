using ArangoDBNetStandard;
using ArangoDBNetStandard.Transport.Http;


namespace Genie.Adapters.Persistence.ArangoDB;

public class ArangoPooledObject
{
    public static ArangoDBClient Client = new ArangoDBClient(HttpApiTransport.UsingBasicAuth(
            new Uri("http://localhost:8529"),
            "genie",
            "admin",
            "pass"));


    public ArangoPooledObject()
    {

    }
}