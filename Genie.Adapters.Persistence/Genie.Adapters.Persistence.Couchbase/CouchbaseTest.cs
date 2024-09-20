using Genie.Utils;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.Couchbase;


public class CouchbaseTest(int payload, ObjectPool<CouchbasePooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;

    public ObjectPool<CouchbasePooledObject> Pool = pool;



    public static void CreateDB()
    {

    }

    public void Write(int i)
    {
        var test = new PersistenceTest
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();
        _ = lease.Collection.UpsertAsync($@"new{i}", test).GetAwaiter().GetResult();

        Pool.Return(lease);
    }


    public void Read(int i)
    {
        var lease = Pool.Get();
        var getResult = lease.Collection.GetAsync($@"new{i}").GetAwaiter().GetResult();
        _ = getResult.ContentAs<PersistenceTest>();

        Pool.Return(lease);
    }
}