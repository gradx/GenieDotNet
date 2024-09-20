
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.Marten;

public class MartenTest(int payload, ObjectPool<MartenPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MartenPooledObject> Pool = pool;

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
        using var session = lease.Store.LightweightSession();

        session.Store(test);
        session.SaveChanges();

        Pool.Return(lease);
    }

    public void Read(int i)
    {
        var lease = Pool.Get();
        using var session = lease.Store.QuerySession();
        var list = session.Query<PersistenceTest>().Where(x => x.Id == $@"new{i}").ToList();
        var result = list.FirstOrDefault();

        Pool.Return(lease);
    }
}