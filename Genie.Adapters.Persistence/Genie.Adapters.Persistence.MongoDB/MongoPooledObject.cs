using Genie.Utils;

using MongoDB.Driver;

namespace Genie.Adapters.Persistence.MongoDB;

public class MongoPooledObject
{
    public readonly MongoClient Client = new("mongodb://mongoadmin:secret@localhost:27017/?authSource=admin");
    public readonly IMongoDatabase Database;
    public readonly IMongoCollection<PersistenceTest> Collection;
    public MongoPooledObject()
    {
        Database = Client.GetDatabase("Northwind");
        Collection = Database.GetCollection<PersistenceTest>("test");
    }
}