
using Humanizer.Localisation;
using Milvus.Client;

namespace Genie.Adapters.Persistence.Milvus;

public class MilvusPooledObject
{
    public MilvusClient Client = new("localhost", 19530, false);
    public MilvusCollection Json { get; set; }
    public MilvusCollection Postal { get; set; }

    public MilvusPooledObject()
    {
        MilvusHealthState result = Client.HealthAsync().GetAwaiter().GetResult();
        
        Json = Client.GetCollection("json_data");
        Json.LoadAsync().GetAwaiter().GetResult();

        Postal = Client.GetCollection("country_postal");
        Postal.LoadAsync().GetAwaiter().GetResult();
    }
}
