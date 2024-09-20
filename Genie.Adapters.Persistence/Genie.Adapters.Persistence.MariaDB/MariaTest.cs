
using Elastic.Clients.Elasticsearch.Graph;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text.Json;

namespace Genie.Adapters.Persistence.MariaDB;

public class MariaTest(int payload, ObjectPool<MariaPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MariaPooledObject> Pool = pool;

    public void CreateDB()
    {
        var lease = Pool.Get();

        MySqlCommand cmd = new MySqlCommand(@"DROP TABLE IF EXISTS benchmarks; 
            CREATE TABLE benchmarks (id TEXT, json LONGTEXT);", lease.Connection);

        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void Write(int i)
    {
        string id = $@"new{i}";
        var test = new PersistenceTest
        {
            Id = id,
            Info = new('-', Payload)
        };

        var lease = Pool.Get();

        MySqlCommand cmd = new MySqlCommand("INSERT INTO benchmarks (id, json) VALUES (@id, @json)", lease.Connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@json", JsonSerializer.Serialize(test));
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void Read(int i)
    {
        string id = $@"new{i}";
        var lease = Pool.Get();

        MySqlCommand cmd = new MySqlCommand("SELECT * FROM benchmarks WHERE id = @id", lease.Connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var result = (string)reader["json"];
            var json = JsonSerializer.Deserialize<PersistenceTest>(result);
        }

        reader.Close();

        Pool.Return(lease);
    }
}