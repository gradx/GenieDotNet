using Raven.Client.Documents;

namespace Genie.Adapters.Persistence.RavenDB;

public class RavenPooledObject
{
    private const string c_Url = "http://localhost:8080";
    public const string c_Database = "Northwind";

    public readonly DocumentStore Store;

    public RavenPooledObject()
    {
        Store = new DocumentStore
        {
            Urls = [c_Url],
            Database = c_Database,
            Conventions = { }
        };

        Store.Initialize();
    }
}