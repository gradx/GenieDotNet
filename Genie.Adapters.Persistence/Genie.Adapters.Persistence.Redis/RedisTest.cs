using Elastic.Clients.Elasticsearch;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Redis;

public class RedisTest(int payload, ObjectPool<RedisPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<RedisPooledObject> Pool = pool;

    public override bool WriteJson(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            string id = $@"new{i}";
            var test = new PersistenceTestModel
            {
                Id = id,
                Info = new('-', Payload)
            };

            var result = lease.Database.StringSet(id, JsonSerializer.Serialize(test));

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
            var json = JsonSerializer.Serialize(message);

            lease.Database.StringSet(message.Id.ToString(), json);
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

            var match = await lease.Database.StringGetAsync(message.Id.ToString());
            var cc = JsonSerializer.Deserialize<CountryPostalCode>(match.ToString());

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

        return result;
    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;

        return result;
    }
}