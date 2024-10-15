
using Confluent.Kafka;
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.Marten;

public class MartenTest(int payload, ObjectPool<MartenPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MartenPooledObject> Pool = pool;

    public MartenTest() : this(JsonSize,new DefaultObjectPool<MartenPooledObject>(new DefaultPooledObjectPolicy<MartenPooledObject>()))
    {

    }


    public override void CreateDB()
    {

    }
    public override void CreateIndex()
    {
        // ??
    }

    public override bool WriteJson(long i)
    {
        bool success = true;
        var test = new PersistenceTestModel
        {
            Id = i.ToString(),
            Info = new('-', Payload)
        };

        var lease = Pool.Get();
        using var session = lease.Store.LightweightSession();

        session.Store(test);
        session.SaveChanges();

        Pool.Return(lease);
        return success;
    }

    public override bool ReadJson(long i)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.QuerySession();
            var list = session.Query<PersistenceTestModel>().Where(x => x.Id == i.ToString()).ToList();
            var match = list.ToList();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }


    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.LightweightSession();

            session.Store(new CountryPostalCodeMarten
            {
                Id = message.Id.ToString(),
                CountryCode = message.CountryCode,
                PostalCode = message.PostalCode ?? "",
                PlaceName = message.PlaceName,
                Latitude = message.Latitude,
                Longitude = message.Longitude
            });
            session.SaveChanges();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public override async Task<bool> UpdatePostal(CountryPostalCode message)
    {
        return await WritePostal(message);
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.QuerySession();
            var list = session.Query<CountryPostalCodeMarten>().Where(x => x.Id == message.Id.ToString()).ToList();
            var match = list.FirstOrDefault();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
    public override async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.QuerySession();
            var list = session.Query<CountryPostalCodeMarten>().Where(x => x.PostalCode == message.PostalCode).ToList();
            var match = list.ToList();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var session = lease.Store.QuerySession();
            var id_match = session.Query<CountryPostalCodeMarten>().Where(x => x.Id == message.Id.ToString()).FirstOrDefault();

            if (id_match != null)
            {
                var postal_query = session.Query<CountryPostalCodeMarten>().Where(x => x.PostalCode == id_match.PostalCode).ToList();
                var results = postal_query.ToList();
            }

        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}