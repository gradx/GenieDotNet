using Genie.Utils;
using Cassandra;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.Cassandra;

public class CassandraTest(int payload, ObjectPool<CassandraPooledObject> pool) : IPersistenceTest
{
    public ObjectPool<CassandraPooledObject> Pool = pool;

    public int Payload { get; set; } = payload;
    

    public void Write(int i)
    {
        var test = new PersistenceTest
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();

        _ = lease.Session.Execute($@"INSERT INTO genie.test(id, json, last_update_timestamp) VALUES('{i}', '{JsonSerializer.Serialize(test)}', toTimeStamp(now()))");


        Pool.Return(lease);
    }

    public void Read(int i)
    {
        var lease = Pool.Get();

        var result = lease.Session.Execute($@"SELECT json FROM genie.test WHERE id = '{i}'");
        var first = result.FirstOrDefault();
        _ = JsonSerializer.Deserialize<PersistenceTest>(Encoding.UTF8.GetBytes((string)first!["json"]));

        Pool.Return(lease);
    }
}