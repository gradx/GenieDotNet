using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Neo4j.Driver;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Neo4j;

public class Neo4jTest(int payload, ObjectPool<Neo4jPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<Neo4jPooledObject> Pool = pool;

    public static void CreateDB()
    {

    }

    public override bool WriteJson(long i)
    {
        bool success = true;
        string id = $@"new{i}";

        var test = new PersistenceTestModel
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

        }).GetAwaiter().GetResult();

        Pool.Return(lease);
        return success;
    }


    public override bool ReadJson(long i)
    {
        bool success = true;
        string id = $@"new{i}";

        var lease = Pool.Get();
        using var session = lease.Driver.AsyncSession();
        var result = session.RunAsync("MATCH (genie:Benchmark:Result { id: $id}) RETURN genie.value", new {id}).GetAwaiter().GetResult();
        var matches = result.ToListAsync().GetAwaiter().GetResult();

        Pool.Return(lease);
        return success;

    }

    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            await lease.Session.ExecuteWriteAsync(async tx =>
            {
                message.CountryCode ??= "";
                message.PostalCode ??= "";
                message.PlaceName ??= "";
                message.Latitude ??= 0;
                message.Longitude ??= 0;

                var result = await tx.RunAsync(
                   @"MERGE (genie:Benchmark:CountryCode {id: $Id, country_code: $CountryCode, postal_code: $PostalCode, 
                                place_name: $PlaceName, latitude: $Latitude, longitude: $Longitude}) RETURN genie:Benchmark:CountryCode",
                   new { message.Id, message.CountryCode, message.PostalCode, message.PlaceName, message.Latitude, message.Longitude });
            });
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
            await lease.Session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (genie:Benchmark:CountryCode { id: $id}) RETURN genie.value", new { message.Id });
                var matches = await result.ToListAsync();

            });
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
            await lease.Session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (genie:Benchmark:CountryCode { id: $id}) RETURN genie.value", new { message.Id });
                var matches = await result.ToListAsync();
            });
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
            await lease.Session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (genie:Benchmark:CountryCode { id: $id}) RETURN genie.value", new { message.Id });
                var matches = await result.ToListAsync();

            });
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}