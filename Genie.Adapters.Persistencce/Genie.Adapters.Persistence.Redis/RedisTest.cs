using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Redis;

public class RedisTest(int payload, ObjectPool<RedisPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<RedisPooledObject> Pool = pool;

    public RedisTest() : this(JsonSize,new DefaultObjectPool<RedisPooledObject>(new DefaultPooledObjectPolicy<RedisPooledObject>()))
    {

    }

    public RedisTest(bool create) : this(JsonSize,new DefaultObjectPool<RedisPooledObject>(new DefaultPooledObjectPolicy<RedisPooledObject>()))
    {
        if (create)
            CreateDB();
    }

    public override void CreateDB()
    {
        
    }

    public override void CreateIndex()
    {
        
    }

    public override bool WriteJson(long i)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            var test = new PersistenceTestModel
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            };

            var match = lease.Database.StringSet(test.Id, JsonSerializer.Serialize(test));

        }
        catch(Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public override bool ReadJson(long i)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            var match = lease.Database.StringGet(i.ToString());
            var cc = JsonSerializer.Deserialize<PersistenceTestModel>(match.ToString());

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
            var match = lease.Database.StringGet(message.Id.ToString());
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
        throw new NotSupportedException();
    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        throw new NotSupportedException();
    }
}