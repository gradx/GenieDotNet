using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Couchbase;


public class CouchbaseTest(int payload, ObjectPool<CouchbasePooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    public ObjectPool<CouchbasePooledObject> Pool = pool;

    public const int Count = 250000;

    public static void CreateDB()
    {

    }

    public override bool WriteJson(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            var test = new PersistenceTestModel
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };


            _ = lease.Collection.UpsertAsync($@"new{i}", test).GetAwaiter().GetResult();

        }
        catch(Exception ex)
        {
            success = false;
        }

        Pool.Return(lease);
        return success;
    }

    public override bool ReadJson(long i)
    {
        return true;
    }



    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            _ = await lease.Collection.InsertAsync(message.Id.ToString(), message);
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            var other = await lease.Collection.GetAsync(message.Id.ToString());
            var match = other.ContentAs<CountryPostalCode>();
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
            var query = await lease.Scope.QueryAsync<JToken>($@"SELECT * FROM genie_bench WHERE postalCode = '{message.PostalCode}';");
            var list = await query.ToListAsync();

            foreach (var match in list)
            {
                var here = JsonSerializer.Deserialize<CountryPostalCode>(match.First.First.ToString(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
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
            var other = await lease.Collection.GetAsync(message.Id.ToString());
            var match = other.ContentAs<CountryPostalCode>();

            var query = await lease.Scope.QueryAsync<JToken>($@"SELECT * FROM genie_bench WHERE postalCode = '{match.PostalCode}';");
            var list = await query.ToListAsync();

            foreach (var item in list)
            {
                var here = JsonSerializer.Deserialize<CountryPostalCode>(item.First.First.ToString(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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