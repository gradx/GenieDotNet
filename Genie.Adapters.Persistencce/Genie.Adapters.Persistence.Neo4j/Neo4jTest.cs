using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using Neo4j.Driver;
using n = Neo4j.Driver;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Genie.Adapters.Persistence.Neo4j;

public class Neo4jTest(int payload, ObjectPool<Neo4jPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<Neo4jPooledObject> Pool = pool;

    public Neo4jTest() : this(JsonSize,new DefaultObjectPool<Neo4jPooledObject>(new DefaultPooledObjectPolicy<Neo4jPooledObject>()))
    {

    }

    public Neo4jTest(bool create) : this(JsonSize,new DefaultObjectPool<Neo4jPooledObject>(new DefaultPooledObjectPolicy<Neo4jPooledObject>()))
    {
        if (create)
            CreateDB();
    }

    public override void CreateDB()
    {
        CreateIndex();
    }

    public override void CreateIndex()
    {
        var lease = Pool.Get();

        lease.Session.RunAsync("CREATE INDEX idx_genie IF NOT EXISTS FOR (n:country_postal) ON (n.postal_code);");

        Pool.Return(lease);
    }

    public override bool WriteJson(long i)
    {
        bool success = true;

        var lease = Pool.Get();

        try
        {
            string id = i.ToString();
            var test = new PersistenceTestModel
            {
                Id = id,
                Info = new('-', Payload)
            };


            var json = JsonSerializer.Serialize(test);

            using var session = lease.Driver.AsyncSession();
            session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync(
                   @"MERGE (genie:Benchmark:json_data {id: $id, value: $json}) RETURN genie:Benchmark:Result",
                   new { id, json });

            }).GetAwaiter().GetResult();
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
            string id = i.ToString();


            using var session = lease.Driver.AsyncSession();
            var result = session.RunAsync("MATCH (genie:Benchmark:json_data { id: $id}) RETURN genie.value", new { id }).GetAwaiter().GetResult();
            var matches = result.ToListAsync().GetAwaiter().GetResult();
            var match = matches.FirstOrDefault();
            var json = JsonSerializer.Deserialize<PersistenceTestModel>((string)match[0]);
        }
        catch(Exception ex)
        {
            success = false;
        }

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
                   @"MERGE (genie:Benchmark:country_postal {id: $Id, country_code: $CountryCode, postal_code: $PostalCode, 
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
            await lease.Session.ExecuteReadAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (genie:Benchmark:country_postal { id: $Id}) RETURN genie", new { message.Id });
                var matches = await result.ToListAsync();
                foreach(var match in matches)
                {
                    var record = match.FirstOrDefault();
                    var node = record.Value.As<INode>();
                    var cc = Get(node);
                }
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
                var result = await tx.RunAsync("MATCH (genie:Benchmark:country_postal { postal_code: $PostalCode}) RETURN genie", new { message.PostalCode });
                var matches = await result.ToListAsync();
                foreach (var match in matches)
                {
                    var record = match.FirstOrDefault();
                    var node = record.Value.As<INode>();
                    var cc = Get(node);
                }
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
                var result = await tx.RunAsync("MATCH (genie:Benchmark:country_postal { id: $Id}) RETURN genie", new { message.Id });
                var matches = await result.ToListAsync();
                CountryPostalCode cc = null;

                foreach (var match in matches)
                {
                    var record = match.FirstOrDefault();
                    var node = record.Value.As<INode>();
                    cc = Get(node);
                    break;
                }

                result = await tx.RunAsync("MATCH (genie:Benchmark:country_postal { postal_code: $PostalCode}) RETURN genie", new { message.PostalCode });
                matches = await result.ToListAsync();

                foreach (var match in matches)
                {
                    var record = match.FirstOrDefault();
                    var node = record.Value.As<INode>();
                    cc = Get(node);
                }
            });
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    private CountryPostalCode Get(INode node)
    {
        return new CountryPostalCode
        {
            Id = (long)node.Properties["id"],
            CountryCode = (string)node.Properties["country_code"],
            PostalCode = (string)node.Properties["postal_code"],
            PlaceName = (string)node.Properties["place_name"],
            Latitude = (double)node.Properties["latitude"],
            Longitude = (double)node.Properties["longitude"],
        };
    }
}