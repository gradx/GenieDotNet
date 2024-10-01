using MongoDB.Driver;

namespace Genie.Adapters.Persistence.MongoDB;

public class MongoPooledObject<T>
{
    public readonly MongoClient Client = new("mongodb://mongoadmin:secret@localhost:27017/?authSource=admin");
    public readonly IMongoDatabase Database;
    public IMongoCollection<T>? Collection;

    public MongoPooledObject()
    {
        Database = Client.GetDatabase("Northwind");
    }

    public void Configure(string collectionName)
    {
        Collection = Database.GetCollection<T>(collectionName);
    }
}
