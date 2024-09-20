using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using MongoDB.Driver;

namespace Genie.Adapters.Persistence.MongoDB
{
    public class MongoTest(int payload, ObjectPool<MongoPooledObject> pool) : IPersistenceTest
    {
        public int Payload { get; set; } = payload;
        readonly ObjectPool<MongoPooledObject> Pool = pool;


        public void Write(int i)
        {
            var test = new PersistenceTest
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };

            var lease = Pool.Get();
            lease.Collection.ReplaceOne(Builders<PersistenceTest>.Filter.Eq("id", $@"new{i}"), test);

            Pool.Return(lease);
        }

        public void Read(int i)
        {
            var lease = Pool.Get();

            var results = lease.Collection.Find(t => t.Id == $@"new{i}").ToList();

            Pool.Return(lease);
        }
    }
}
