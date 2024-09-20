
using Milvus.Client;

namespace Genie.Adapters.Persistence.Milvus;

public class MilvusPooledObject
{
    public const string CollectionName = "book";
    public static MilvusClient Client { get; }
    public static MilvusCollection? Collection { get; set; }

    static MilvusPooledObject()
    {
        string Host = "localhost";
        int Port = 19530; // This is Milvus's default port
        bool UseSsl = false; // Default value is false
        string Database = "my_database"; // Defaults to the default Milvus database

        // See documentation for other constructor paramters
        Client = new MilvusClient(Host, Port, UseSsl);
        MilvusHealthState result = Client.HealthAsync().GetAwaiter().GetResult();

        CreateDB(true);
    }

    static void CreateDB(bool dropExisting)
    {
        MilvusCollection collection = Client.GetCollection(CollectionName);

        //Check if this collection exists
        var hasCollection = Client.HasCollectionAsync(CollectionName).GetAwaiter().GetResult();

        if (hasCollection && dropExisting)
        {
            collection.DropAsync().GetAwaiter().GetResult();
            Console.WriteLine("Drop collection {0}", CollectionName);
        }

        Collection = Client.CreateCollectionAsync(
                    CollectionName,
                    [
                        FieldSchema.CreateVarchar("test_id", 256, isPrimaryKey:true),
                        FieldSchema.CreateJson("json"),
                        FieldSchema.CreateFloatVector("vector", 2),
                    ]
                ).GetAwaiter().GetResult();

        Collection.CreateIndexAsync(
            "vector",
            //MilvusIndexType.IVF_FLAT,//Use MilvusIndexType.IVF_FLAT.
            IndexType.AutoIndex,//Use MilvusIndexType.AUTOINDEX when you are using zilliz cloud.
            SimilarityMetricType.L2).GetAwaiter().GetResult();

        Collection.LoadAsync().GetAwaiter().GetResult();
    }
}