using Genie.Utils;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using StackExchange.Redis;
using Cassandra;

namespace Genie.Adapters.Persistence.Scylla;

public class ScyllaTest(int payload, ObjectPool<ScyllaPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<ScyllaPooledObject> Pool = pool;
    
    public void CreateDB()
    {
        var lease = Pool.Get();

        _ = lease.Session.Execute("DROP TABLE genie.test;");

        _ = lease.Session.Execute(@"CREATE TABLE IF NOT EXISTS genie.test (
            id text PRIMARY KEY,
            json text,
            last_update_timestamp timestamp
            );");


        Pool.Return(lease);
    }
    public override bool WriteJson(long i)
    {
        bool success = true;
        var test = new PersistenceTestModel
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };

        var lease = Pool.Get();
        _ = lease.Session.Execute($@"INSERT INTO genie.test(id, json, last_update_timestamp) VALUES('{i}', '{JsonSerializer.Serialize(test)}', toTimeStamp(now()))");

        Pool.Return(lease);
        return success;
    }

    public override bool ReadJson(long i)
    {
        return true;
    }


    public async Task<bool> CreatePostalDB()
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            lease.Session.Execute("DROP TABLE IF EXISTS genie.country_data;");

            _ = lease.Session.Execute(@"CREATE TABLE IF NOT EXISTS genie.country_data (
                id bigint PRIMARY KEY,
                country_code text,
                postal_code text,
                place_name text,
                latitude double,
                longitude double,
            );");

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
            var sql = $@"INSERT INTO genie.country_data(id, country_code, postal_code, place_name, latitude, longitude) VALUES (?, ?, ?, ?, ?, ?)";
            var insertSql = await lease.Session.PrepareAsync(sql);
            await lease.Session.ExecuteAsync(insertSql.Bind((long)message.Id, message.CountryCode, message.PostalCode, message.PlaceName, message.Latitude, message.Longitude));

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
            var sql = $@"SELECT * FROM genie.country_data WHERE id = ?";
            var read = await lease.Session.PrepareAsync(sql);
            var match = await lease.Session.ExecuteAsync(read.Bind((long)message.Id));
            var first = match.FirstOrDefault();

            var cc = new CountryPostalCode
            {
                Id = Convert.ToInt32(first["id"]),
                CountryCode = first["country_code"] is DBNull ? null : (string)first["country_code"],
                PostalCode = first["postal_code"] is DBNull ? null : (string)first["postal_code"],
                PlaceName = first["place_name"] is DBNull ? null : (string)first["place_name"],
                Latitude = first["latitude"] is DBNull ? null : (double)first["latitude"],
                Longitude = first["longitude"] is DBNull ? null : (double)first["longitude"],
            };

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
            var sql = $@"SELECT * FROM genie.country_data WHERE postal_code = ? ALLOW FILTERING";
            var read = await lease.Session.PrepareAsync(sql);
            var match = await lease.Session.ExecuteAsync(read.Bind(message.PostalCode));
            var first = match.FirstOrDefault();

            var cc = new CountryPostalCode
            {
                Id = Convert.ToInt32(first["id"]),
                CountryCode = first["country_code"] is DBNull ? null : (string)first["country_code"],
                PostalCode = first["postal_code"] is DBNull ? null : (string)first["postal_code"],
                PlaceName = first["place_name"] is DBNull ? null : (string)first["place_name"],
                Latitude = first["latitude"] is DBNull ? null : (double)first["latitude"],
                Longitude = first["longitude"] is DBNull ? null : (double)first["longitude"],
            };

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

            var id_sql = await lease.Session.PrepareAsync($@"SELECT * FROM genie.country_data WHERE id = ?");
            var id_response = await lease.Session.ExecuteAsync(id_sql.Bind((long)message.Id));
            var id_result = id_response.FirstOrDefault();

            var sql = $@"SELECT * FROM genie.country_data WHERE postal_code = ? ALLOW FILTERING";
            var read = await lease.Session.PrepareAsync(sql);
            var response = await lease.Session.ExecuteAsync(read.Bind(id_result["postal_code"]));
            var results = response.ToList();

            foreach (var a in results)
            {
                var cc2 = new CountryPostalCode
                {
                    Id = Convert.ToInt32(id_result["id"]),
                    CountryCode = a["country_code"] is DBNull ? null : (string)a["country_code"],
                    PostalCode = a["postal_code"] is DBNull ? null : (string)a["postal_code"],
                    PlaceName = a["place_name"] is DBNull ? null : (string)a["place_name"],
                    Latitude = a["latitude"] is DBNull ? null : (double)a["latitude"],
                    Longitude = a["longitude"] is DBNull ? null : (double)a["longitude"],
                };
            }

        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}