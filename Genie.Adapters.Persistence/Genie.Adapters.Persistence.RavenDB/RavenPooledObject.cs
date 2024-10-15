using Raven.Client.Documents;
using Sparrow.Json;

namespace Genie.Adapters.Persistence.RavenDB;

public class RavenPooledObject
{
    private const string c_Url = "http://localhost:8080";
    public const string Json = "json_data";
    public const string Postal = "country_postal";

    public readonly DocumentStore JsonStore;
    public readonly DocumentStore PostalStore;

    public RavenPooledObject()
    {
        JsonStore = new DocumentStore
        {
            Urls = [c_Url],
            Database = Json,
            Conventions = { }
        };

        JsonStore.Initialize();

        PostalStore = new DocumentStore
        {
            Urls = [c_Url],
            Database = Postal,
            Conventions = { }
        };

        PostalStore.Initialize();
    }       
}