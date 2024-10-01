using Elastic.Clients.Elasticsearch;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using MySqlConnector;
using System.Text.Json;

namespace Genie.Adapters.Persistence.MariaDB;

public class MariaTest(int payload, ObjectPool<MariaPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MariaPooledObject> Pool = pool;

    public void CreateDB()
    {
        var lease = Pool.Get();

        MySqlCommand cmd = new MySqlCommand(@"DROP TABLE IF EXISTS benchmarks; 
            CREATE TABLE benchmarks (id VARCHAR(255) PRIMARY KEY, json LONGTEXT);", lease.Connection);

        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void CreateMySqlDB()
    {
        var lease = Pool.Get();

        MySqlCommand cmd = new MySqlCommand(@"DROP TABLE IF EXISTS benchmarks; 
            CREATE TABLE benchmarks (id VARCHAR(255) PRIMARY KEY, json JSON);", lease.Connection);

        cmd.ExecuteNonQuery();

        Pool.Return(lease);
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

        var lease = Pool.Get();

        MySqlCommand cmd = new MySqlCommand("INSERT INTO benchmarks (id, json) VALUES (@id, @json) ON DUPLICATE KEY UPDATE json=@json", lease.Connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@json", JsonSerializer.Serialize(test));
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
        return success;
    }

    public override bool ReadJson(long i)
    {
        return true;
    }

    public void CreateDBPostal()
    {
        var lease = Pool.Get();

        MySqlCommand cmd = new MySqlCommand(@"DROP TABLE IF EXISTS country_postal_latency; 
                CREATE TABLE country_postal (id BIGINT PRIMARY KEY, country_code TINYTEXT, postal_code TINYTEXT, place_name TINYTEXT, latitude DOUBLE, longitude DOUBLE)", lease.Connection);

        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public async Task<bool> CreatePostalDB()
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            MySqlCommand cmd = new MySqlCommand(@"DROP TABLE IF EXISTS country_postal; 
                CREATE TABLE country_postal (id BIGINT PRIMARY KEY, country_code TINYTEXT, postal_code TINYTEXT, place_name TINYTEXT, latitude DOUBLE, longitude DOUBLE)", lease.Connection);

            await cmd.ExecuteNonQueryAsync();
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
            MySqlCommand cmd = new MySqlCommand(@"INSERT INTO country_postal (id, country_code, postal_code, place_name, 
                        latitude, longitude) VALUES (@id, @country_code, @postal_code, @place_name, @latitude, @longitude) 
                    ON DUPLICATE KEY UPDATE country_code=@country_code, postal_code = @postal_code, place_name = @place_name,
                        latitude = @latitude, longitude = @longitude;", lease.Connection);
            cmd.Parameters.AddWithValue("@id", message.Id);
            cmd.Parameters.AddWithValue("@country_code", message.CountryCode);
            cmd.Parameters.AddWithValue("@postal_code", message.PostalCode);
            cmd.Parameters.AddWithValue("@place_name", message.PlaceName);
            cmd.Parameters.AddWithValue("@latitude", message.Latitude);
            cmd.Parameters.AddWithValue("@longitude", message.Longitude);
            cmd.ExecuteNonQuery(); // Async is slower
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
            MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM country_postal WHERE id = @id;", lease.Connection);
            cmd.Parameters.AddWithValue("@id", message.Id);
            var reader = cmd.ExecuteReader();

            reader.Read();
            var cc = new CountryPostalCode
            {
                Id = Convert.ToInt32(reader["id"]),
                CountryCode = reader["country_code"] is DBNull ? null : (string)reader["country_code"],
                PostalCode = reader["postal_code"] is DBNull ? null : (string)reader["postal_code"],
                PlaceName = reader["place_name"] is DBNull ? null : (string)reader["place_name"],
                Latitude = reader["latitude"] is DBNull ? null : (double)reader["latitude"],
                Longitude = reader["longitude"] is DBNull ? null : (double)reader["longitude"],
            };

            await reader.DisposeAsync();
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
            MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM country_postal WHERE postal_code = @postal_code;", lease.Connection);
            cmd.Parameters.AddWithValue("@postal_code", message.PostalCode);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var cc = new CountryPostalCode
                {
                    Id = Convert.ToInt32(reader["id"]),
                    CountryCode = reader["country_code"] is DBNull ? null : (string)reader["country_code"],
                    PostalCode = reader["postal_code"] is DBNull ? null : (string)reader["postal_code"],
                    PlaceName = reader["place_name"] is DBNull ? null : (string)reader["place_name"],
                    Latitude = reader["latitude"] is DBNull ? null : (double)reader["latitude"],
                    Longitude = reader["longitude"] is DBNull ? null : (double)reader["longitude"],
                };
            }

            await reader.DisposeAsync();
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
            MySqlCommand cmd = new MySqlCommand(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.id = @id AND c.id != c2.id;", lease.Connection);
            cmd.Parameters.AddWithValue("@id", message.Id);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var cc = new CountryPostalCode
                {
                    Id = Convert.ToInt32(reader["id"]),
                    CountryCode = reader["country_code"] is DBNull ? null : (string)reader["country_code"],
                    PostalCode = reader["postal_code"] is DBNull ? null : (string)reader["postal_code"],
                    PlaceName = reader["place_name"] is DBNull ? null : (string)reader["place_name"],
                    Latitude = reader["latitude"] is DBNull ? null : (double)reader["latitude"],
                    Longitude = reader["longitude"] is DBNull ? null : (double)reader["longitude"],
                };
            }

            await reader.DisposeAsync();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}