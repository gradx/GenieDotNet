
using Genie.Adapters.Persistence.CockroachDB;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System.Text.Json;

namespace Genie.Adapters.Persistence.CockroachDB;

public class CockroachTest(int payload, ObjectPool<CockroachPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    public ObjectPool<CockroachPooledObject> Pool = pool;

    public void CreateDB()
    {
        var lease = Pool.Get();
        using var cmd = new NpgsqlCommand(@"DROP TABLE IF EXISTS bench", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE bench(id text NOT NULL, 
                json JSONB NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))";


        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public bool Write(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            var test = new PersistenceTest
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };


            using var cmd = new NpgsqlCommand("INSERT INTO bench (id,json) VALUES(@p,@json) ON CONFLICT(id) DO UPDATE SET json = @json", lease.Connection);
            cmd.Parameters.AddWithValue("p", test.Id);
            cmd.Parameters.AddWithValue("json", JsonSerializer.Serialize(test));
            cmd.ExecuteNonQuery();
            cmd.Dispose();

        }
        catch(Exception ex)
        {
            success = false;    
        }


        Pool.Return(lease);
        return success;
    }

    public bool Read(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        using var cmd = new NpgsqlCommand("SELECT json FROM bench WHERE id = @id", lease.Connection);
        cmd.Parameters.AddWithValue("id", $@"new{i}");

        var json = (string)cmd.ExecuteScalar();
        var result = JsonSerializer.Deserialize<PersistenceTest>(Encoding.UTF8.GetBytes(json));
        
        Pool.Return(lease);
        return success;
    }

    public async Task<bool> CreatePostalDB()
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS country_postal; CREATE TABLE country_postal(id INT PRIMARY KEY, country_code STRING, postal_code STRING, place_name STRING, latitude double precision, longitude double precision);", lease.Connection);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"INSERT INTO country_postal (id, country_code, postal_code, place_name, latitude, longitude) VALUES  (@id, @country_code, @postal_code, @place_name, @latitude, @longitude)  ON CONFLICT(id) 
                    DO UPDATE SET country_code=@country_code, postal_code = @postal_code, place_name = @place_name, latitude = @latitude, longitude = @longitude", lease.Connection);

            cmd.Parameters.AddWithValue("@id", message.Id);
            cmd.Parameters.Add(new NpgsqlParameter("@country_code", NpgsqlDbType.Varchar) { Value = message.CountryCode == null ? DBNull.Value : message.CountryCode });
            cmd.Parameters.Add(new NpgsqlParameter("@postal_code", NpgsqlDbType.Varchar) { Value = message.PostalCode == null ? DBNull.Value : message.PostalCode });
            cmd.Parameters.Add(new NpgsqlParameter("@place_name", NpgsqlDbType.Varchar) { Value = message.PlaceName == null ? DBNull.Value : message.PlaceName });
            cmd.Parameters.Add(new NpgsqlParameter("@latitude", NpgsqlDbType.Double) { Value = message.Latitude == null ? DBNull.Value : message.Latitude });
            cmd.Parameters.Add(new NpgsqlParameter("@longitude", NpgsqlDbType.Double) { Value = message.Longitude == null ? DBNull.Value : message.Longitude });
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }

    public async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"SELECT * FROM country_postal WHERE id = @id", lease.Connection);

            cmd.Parameters.AddWithValue("@id", message.Id);

            var reader = cmd.ExecuteReader(); // nonasync is slower
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
    public async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"SELECT * FROM country_postal WHERE postal_code = @postal_code", lease.Connection);

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

    public async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.id = @id AND c.id != c2.id", lease.Connection);

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