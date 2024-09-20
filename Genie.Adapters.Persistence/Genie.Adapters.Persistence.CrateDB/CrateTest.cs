
using Genie.Adapters.Persistence.CrateDB;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Npgsql;
using System.Text;
using System.Text.Json;

namespace Genie.Adapters.Persistence.CrateDB;

public class CrateTest(int payload, ObjectPool<CratePooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;

    readonly ObjectPool<CratePooledObject> Pool = pool;


    public void CreateDB()
    {
        var lease = Pool.Get();

        using var cmd = new NpgsqlCommand("CREATE TABLE bench(id text NOT NULL, json text NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))", lease.Connection);
        cmd.ExecuteNonQuery();


        Pool.Return(lease);
    }

    public void Write(int i)
    {
        var test = new PersistenceTest
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();

        using var cmd = new NpgsqlCommand("INSERT INTO bench (id,json) VALUES(@p,@json) ON CONFLICT(id) DO UPDATE SET json = @json", lease.Connection);
        cmd.Parameters.AddWithValue("p", test.Id);
        cmd.Parameters.AddWithValue("json", JsonSerializer.Serialize(test));
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }


    public void Read(int i)
    {
        var lease = Pool.Get();

        using var cmd = new NpgsqlCommand("SELECT * FROM bench WHERE id = @id", lease.Connection);
        cmd.Parameters.AddWithValue("id", $@"new{i}");

        using var reader = cmd.ExecuteReader();
        reader.Read();
        var result = JsonSerializer.Deserialize<PersistenceTest>(Encoding.UTF8.GetBytes((string)reader["json"]));
        reader.Close();

        Pool.Return(lease);
    }
}