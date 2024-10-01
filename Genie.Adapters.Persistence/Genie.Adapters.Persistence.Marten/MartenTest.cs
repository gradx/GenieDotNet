
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

    public bool Write(long i)
    {
        bool success = true;
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
        return success;
    }

    public bool Read(long i)
    {
        bool success = true;
        var lease = Pool.Get();
        using var session = lease.Store.QuerySession();
        var list = session.Query<PersistenceTest>().Where(x => x.Id == $@"new{i}").ToList();
        var result = list.FirstOrDefault();

        Pool.Return(lease);
        return success;
    }

    public async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.LightweightSession();

            session.Store(message);
            session.SaveChanges();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.QuerySession();
            var list = session.Query<CountryPostalCode>().Where(x => x.Id == message.Id).ToList();
            var match = list.FirstOrDefault();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
    public async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.QuerySession();
            var list = session.Query<CountryPostalCode>().Where(x => x.PostalCode == message.PostalCode).ToList();
            var match = list.ToList();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.QuerySession();
            var id_match = session.Query<CountryPostalCode>().Where(x => x.Id == message.Id).FirstOrDefault();

            var postal_query = session.Query<CountryPostalCode>().Where(x => x.PostalCode == id_match.PostalCode).ToList();
            var results = postal_query.ToList();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}