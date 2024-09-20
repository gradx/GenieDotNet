
using Aerospike.Client;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Aerospike;

public class AerospikeTest(int payload, ObjectPool<AerospikePooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    public ObjectPool<AerospikePooledObject> Pool => pool;

    readonly WritePolicy WritePolicy = new();

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

        var bin = new Bin("benchmark", JsonSerializer.Serialize(test));
        var key = new Key("test", "set", $@"new{i}");

        var lease = Pool.Get();
        var result = lease.Client.Operate(WritePolicy, key, Operation.Put(bin));

        Pool.Return(lease);
    }

    public void Read(int i)
    {
        var key = new Key("test", "set", $@"new{i}");

        var lease = Pool.Get();
        var result = lease.Client.Operate(WritePolicy, key, Operation.Get());

        Pool.Return(lease);
    }
}