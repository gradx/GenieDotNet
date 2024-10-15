
using Aerospike.Client;
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Aerospike;

public class AerospikeTest(int payload, ObjectPool<AerospikePooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    public ObjectPool<AerospikePooledObject> Pool => pool;

    readonly WritePolicy WritePolicy = new();


    public AerospikeTest() : this(JsonSize, new DefaultObjectPool<AerospikePooledObject>(new DefaultPooledObjectPolicy<AerospikePooledObject>()))
    {

    }

    public AerospikeTest(bool create) : this(JsonSize, new DefaultObjectPool<AerospikePooledObject>(new DefaultPooledObjectPolicy<AerospikePooledObject>()))
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
        bool success = true;
        var lease = Pool.Get();

        try
        {
            var test = new PersistenceTestModel
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            };
            Write<PersistenceTestModel>("test", "set", "json_data", i.ToString(), test);

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
        bool success = true;
        var lease = Pool.Get();

        try
        {
            var result = Read<PersistenceTestModel>("test", "set", "json_data", i.ToString());
        }
        catch (Exception ex)
        {
            success = false;
        }

        Pool.Return(lease);
        return success;
    }


    public void Write<T>(string ns, string set, string table, string key, T data)
    {
        var lease = Pool.Get();
        var bin = new Bin(table, JsonSerializer.Serialize(data));
        var aero_key = new Key(ns, set, key);

        var result = lease.Client.Operate(WritePolicy, aero_key, Operation.Put(bin));

        Pool.Return(lease);
    }

    public T? Read<T>(string ns, string set, string table, string key)
    {
        var aero_key = new Key(ns, set, key);

        var lease = Pool.Get();
        var result = lease.Client.Operate(WritePolicy, aero_key, Operation.Get());

        Pool.Return(lease);

        return JsonSerializer.Deserialize<T>(result.GetString(table));
    }


    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();
        try
        {
            Write<CountryPostalCode>("test", "id", "country_data", message.Id.ToString(), message);
        }
        catch(Exception ex)
        {
            success = false;
        }


        pool.Return(lease);
        return success;
    }

    public override async Task<bool> UpdatePostal(CountryPostalCode message)
    {
        return await WritePostal(message);
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var result = Read<CountryPostalCode>("test", "id", "country_data", message.Id.ToString());
        }
        catch (Exception ex)
        {
            success = false;
        }

        pool.Return(lease);
        return success;
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