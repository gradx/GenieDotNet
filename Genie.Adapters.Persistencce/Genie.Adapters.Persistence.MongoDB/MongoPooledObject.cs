using Genie.Core.Persistence;
using MongoDB.Driver;
using System.Collections;

namespace Genie.Adapters.Persistence.MongoDB;

public class MongoPooledObject
{
    public readonly MongoClient Client = new("mongodb://mongoadmin:secret@localhost:27017/?authSource=admin");
    public readonly IMongoDatabase Database;
    public IMongoCollection<PersistenceTestModel> Json;
    public IMongoCollection<CountryPostalCode> Postal;

    public MongoPooledObject()
    {
        Database = Client.GetDatabase("Northwind");

        Json = Database.GetCollection<PersistenceTestModel>("json_data");
        Postal = Database.GetCollection<CountryPostalCode>("country_postal");
    }
}
