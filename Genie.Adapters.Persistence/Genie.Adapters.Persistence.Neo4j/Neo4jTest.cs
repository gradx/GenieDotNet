using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Neo4j.Driver;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Neo4j;

public class Neo4jTest(int payload, ObjectPool<Neo4jPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<Neo4jPooledObject> Pool = pool;

    public static void CreateDB()
    {

    }

    public void Write(int i)
    {
        string id = $@"new{i}";

        var test = new PersistenceTest
        {
            Id = id,
            Info = new('-', Payload)
        };

        var json = JsonSerializer.Serialize(test);

        var lease = Pool.Get();
        using var session = lease.Driver.AsyncSession();
        session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(
               @"MERGE (genie:Benchmark:Result {id: $id, value: $json}) RETURN genie:Benchmark:Result",
               new { id, json });

            var record = await result.SingleAsync();
        }).GetAwaiter().GetResult();

        Pool.Return(lease);
    }


    public void Read(int i)
    {
        string id = $@"new{i}";

        var lease = Pool.Get();
        using var session = lease.Driver.AsyncSession();
        var result = session.RunAsync("MATCH (genie:Benchmark:Result { id: $id}) RETURN genie.value", new {id}).GetAwaiter().GetResult();
        var matches = result.ToListAsync().GetAwaiter().GetResult();

        Pool.Return(lease);

    }
}