
using Elastic.Clients.Elasticsearch;
using Genie.Adapters.Persistence.CrateDB;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Npgsql;
using NpgsqlTypes;
using StackExchange.Redis;
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
        using var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS bench", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE TABLE bench(id text NOT NULL, json OBJECT NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))";
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

            await using var command = lease.DataSource.CreateCommand("DROP TABLE IF EXISTS country_postal;");
            await command.ExecuteNonQueryAsync();

            command.CommandText = @"CREATE TABLE country_postal (
                  postal_pk BIGINT PRIMARY KEY,
                  country_code VARCHAR (255), 
                  postal_code VARCHAR (255) INDEX using plain,
                  place_name VARCHAR(255), 
                  latitude double precision,
                  longitude double precision
                );";

            command.ExecuteNonQuery();
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
            await using (var cmd = lease.DataSource.CreateCommand(@"INSERT INTO 
                    country_postal(postal_pk,country_code,postal_code,place_name,latitude,longitude) 
                    VALUES($1,$2,$3,$4,$5,$6)"))
            {
                cmd.Parameters.Add(new() { Value = message.Id });
                cmd.Parameters.Add(new() { Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.Latitude == null ? DBNull.Value : message.Latitude, NpgsqlDbType = NpgsqlDbType.Double });
                cmd.Parameters.Add(new() { Value = message.Longitude == null ? DBNull.Value : message.Longitude, NpgsqlDbType = NpgsqlDbType.Double });
                await cmd.ExecuteNonQueryAsync();
            }
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
            await using (var cmd = lease.DataSource.CreateCommand(@"SELECT * FROM country_postal WHERE postal_pk = $1"))
            {
                cmd.Parameters.Add(new() { Value = (long)message.Id, NpgsqlDbType = NpgsqlDbType.Bigint });
                var reader = await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();

                var cc = new CountryPostalCode
                {
                    Id = Convert.ToInt32(reader["postal_pk"]),
                    CountryCode = reader["country_code"] is DBNull ? null : (string)reader["country_code"],
                    PostalCode = reader["postal_code"] is DBNull ? null : (string)reader["postal_code"],
                    PlaceName = reader["place_name"] is DBNull ? null : (string)reader["place_name"],
                    Latitude = reader["latitude"] is DBNull ? null : (double)reader["latitude"],
                    Longitude = reader["longitude"] is DBNull ? null : (double)reader["longitude"],
                };

                await reader.DisposeAsync();
            }
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
            await using (var cmd = lease.DataSource.CreateCommand(@"SELECT * FROM country_postal WHERE postal_code = $1"))
            {
                cmd.Parameters.Add(new() { Value = message.PostalCode, NpgsqlDbType = NpgsqlDbType.Text });
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var cc = new CountryPostalCode
                    {
                        Id = Convert.ToInt32(reader["postal_pk"]),
                        CountryCode = reader["country_code"] is DBNull ? null : (string)reader["country_code"],
                        PostalCode = reader["postal_code"] is DBNull ? null : (string)reader["postal_code"],
                        PlaceName = reader["place_name"] is DBNull ? null : (string)reader["place_name"],
                        Latitude = reader["latitude"] is DBNull ? null : (double)reader["latitude"],
                        Longitude = reader["longitude"] is DBNull ? null : (double)reader["longitude"],
                    };

                }

                await reader.DisposeAsync();
            }
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
            await using (var cmd = lease.DataSource.CreateCommand(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.postal_pk = $1 AND c.postal_pk != c2.postal_pk"))
            {
                cmd.Parameters.Add(new() { Value = (long)message.Id, NpgsqlDbType = NpgsqlDbType.Bigint });
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var cc = new CountryPostalCode
                    {
                        Id = Convert.ToInt32(reader["postal_pk"]),
                        CountryCode = reader["country_code"] is DBNull ? null : (string)reader["country_code"],
                        PostalCode = reader["postal_code"] is DBNull ? null : (string)reader["postal_code"],
                        PlaceName = reader["place_name"] is DBNull ? null : (string)reader["place_name"],
                        Latitude = reader["latitude"] is DBNull ? null : (double)reader["latitude"],
                        Longitude = reader["longitude"] is DBNull ? null : (double)reader["longitude"],
                    };
                }

                await reader.DisposeAsync();
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