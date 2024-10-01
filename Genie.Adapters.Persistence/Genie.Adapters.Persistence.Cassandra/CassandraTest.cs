using Genie.Utils;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Cassandra;

namespace Genie.Adapters.Persistence.Cassandra;

public class CassandraTest(int payload, ObjectPool<CassandraPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public ObjectPool<CassandraPooledObject> Pool = pool;

    public int Payload { get; set; } = payload;
    
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

    public void LoadCountryData()
    {
        var lease = Pool.Get();
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
            _ = lease.Session.Execute(@"DROP TABLE IF EXISTS genie.country_data;");

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
            var read = await lease.Session.PrepareAsync("SELECT * FROM genie.country_data WHERE id = ?");
            var match = await lease.Session.ExecuteAsync(read.Bind((long)message.Id));
            var first = match.FirstOrDefault()!;

            var countryCode = new CountryPostalCode
            {
                Id = Convert.ToInt32(first["id"]),
                CountryCode = first["country_code"] is DBNull ? null : (string)first["country_code"],
                PostalCode = first["postal_code"] is DBNull ? null : (string)first["postal_code"],
                PlaceName = first["place_name"] is DBNull ? null : (string)first["place_name"],
                Latitude = first["latitude"] is DBNull ? null : (double)first["latitude"],
                Longitude = first["longitude"] is DBNull ? null : (double)first["longitude"]
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
            var result2 = await lease.Session.ExecuteAsync(read.Bind(message.PostalCode));

            var first = result2.FirstOrDefault()!;

            var countryCode = new CountryPostalCode
            {
                Id = Convert.ToInt32(first["id"]),
                CountryCode = first["country_code"] is DBNull ? null : (string)first["country_code"],
                PostalCode = first["postal_code"] is DBNull ? null : (string)first["postal_code"],
                PlaceName = first["place_name"] is DBNull ? null : (string)first["place_name"],
                Latitude = first["latitude"] is DBNull ? null : (double)first["latitude"],
                Longitude = first["longitude"] is DBNull ? null : (double)first["longitude"]
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
            var read_id = await lease.Session.PrepareAsync("SELECT * FROM genie.country_data WHERE id = ?");
            var id_result = await lease.Session.ExecuteAsync(read_id.Bind((long)message.Id));
            var first = id_result.FirstOrDefault()!;


            var sql = $@"SELECT * FROM genie.country_data WHERE postal_code = ? ALLOW FILTERING";
            var postal_statement = await lease.Session.PrepareAsync(sql);
            var postal_response = await lease.Session.ExecuteAsync(postal_statement.Bind((string)first["postal_code"]));

            var postal_results = postal_response.ToList();

            foreach (var match in postal_results)
            {
                var countryCode = new CountryPostalCode
                {
                    Id = Convert.ToInt32(first["id"]),
                    CountryCode = match["country_code"] is DBNull ? null : (string)match["country_code"],
                    PostalCode = match["postal_code"] is DBNull ? null : (string)match["postal_code"],
                    PlaceName = match["place_name"] is DBNull ? null : (string)match["place_name"],
                    Latitude = match["latitude"] is DBNull ? null : (double)match["latitude"],
                    Longitude = match["longitude"] is DBNull ? null : (double)match["longitude"]
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