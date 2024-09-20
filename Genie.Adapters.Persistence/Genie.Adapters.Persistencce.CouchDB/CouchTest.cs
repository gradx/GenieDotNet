using CouchDB.Driver;
using CouchDB.Driver.Options;
using CouchDB.Driver.Types;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.CouchDB;

public class CouchPersistenceTest : CouchDocument
{
    public string Info { get; set; }
}


public class CouchTest(int payload, ObjectPool<CouchPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    public ObjectPool<CouchPooledObject> Pool = pool;


    public void Write(int i)
    {
        var lease = Pool.Get();
        var result = CouchPooledObject.Database.FindAsync($@"new{i}").GetAwaiter().GetResult();

        result ??= new CouchPersistenceTest
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };

        CouchPooledObject.Database.AddOrUpdateAsync(result).GetAwaiter().GetResult();

        Pool.Return(lease);
    }

    public void Read(int i)
    {
        var lease = Pool.Get();
        var result = CouchPooledObject.Database.FindAsync($@"new{i}").GetAwaiter().GetResult();

        Pool.Return(lease);
    }
}