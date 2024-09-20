using ArangoDBNetStandard;
using ArangoDBNetStandard.Transport.Http;


namespace Genie.Adapters.Persistence.ArangoDB;

public class ArangoPooledObject
{
    public readonly ArangoDBClient Client;

    public ArangoPooledObject()
    {
        var transport = HttpApiTransport.UsingBasicAuth(
            new Uri("http://localhost:8529"),
            "genie",
            "admin",
            "pass");

        Client = new ArangoDBClient(transport);
    }
}