
using Aerospike.Client;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Genie.Adapters.Persistence.Aerospike;

public class AerospikeTest(int payload, ObjectPool<AerospikePooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    public ObjectPool<AerospikePooledObject> Pool => pool;

    readonly WritePolicy WritePolicy = new();


    public AerospikeTest() : this(4000, new DefaultObjectPool<AerospikePooledObject>(new DefaultPooledObjectPolicy<AerospikePooledObject>()))
    {

    }

    public static void CreateDB()
    {

    }

    public bool Write(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            var test = new PersistenceTest
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };

            var bin = new Bin("benchmark", JsonSerializer.Serialize(test));
            var key = new Key("test", "set", $@"new{i}");


            var result = lease.Client.Operate(WritePolicy, key, Operation.Put(bin));

        }
        catch(Exception ex)
        {
            success = false;
        }

        Pool.Return(lease);
        return success;
    }

    public bool Read(long i)
    {
        bool success = true;
        var key = new Key("test", "set", $@"new{i}");

        var lease = Pool.Get();
        var result = lease.Client.Operate(WritePolicy, key, Operation.Get());

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


    public async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();
        try
        {

            var bin = new Bin("country_codes", JsonSerializer.Serialize(message));
            var aero_key1 = new Key("test", "id", message.Id.ToString());
            var op = Operation.Put(bin);

            lease.Client.Operate(WritePolicy, aero_key1, op);
        }
        catch(Exception ex)
        {
            success = false;
        }


        pool.Return(lease);
        return success;
    }

    public async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool success = true;
        var lease = pool.Get();

        try
        {
            var aero_key = new Key("test", "id", message.Id.ToString());

            var result = lease.Client.Operate(WritePolicy, aero_key, Operation.Get());

            var cc = JsonSerializer.Deserialize<CountryPostalCode>((string)result.bins.First().Value);
        }
        catch (Exception ex)
        {
            success = false;
        }

        pool.Return(lease);
        return success;
    }

    public async Task<bool> QueryPostal(CountryPostalCode message)
    {
        return true;
    }

    public async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        return true;
    }
}