
using Genie.Core.Persistence;
using Humanizer.Localisation;
using MaxMind.Db;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.ObjectPool;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Genie.Adapters.Persistence.CrateDB;

public class CrateTest(int payload, ObjectPool<CratePooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    readonly ObjectPool<CratePooledObject> Pool = pool;

    public CrateTest() : this(JsonSize,new DefaultObjectPool<CratePooledObject>(new DefaultPooledObjectPolicy<CratePooledObject>()))
    {

    }

    public CrateTest(bool create) : this(JsonSize,new DefaultObjectPool<CratePooledObject>(new DefaultPooledObjectPolicy<CratePooledObject>()))
    {
        if (create)
            CreateDB();
    }


    public override void CreateDB()
    {
        CreateJsonDB();
        CreatePostalDB();
    }

    public void CreateJsonDB()
    {
        var lease = Pool.Get();

        using var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS json_data", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE TABLE json_data(id text NOT NULL, json OBJECT NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))";
        cmd.ExecuteNonQuery();


        Pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        using var command = lease.DataSource.CreateCommand("DROP TABLE IF EXISTS country_postal;");
        command.ExecuteNonQuery();

        command.CommandText = @"CREATE TABLE country_postal (
                  id BIGINT PRIMARY KEY,
                  country_code VARCHAR (255), 
                  postal_code VARCHAR (255) INDEX using plain,
                  place_name VARCHAR(255), 
                  latitude double precision,
                  longitude double precision
                );";

        command.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public override void CreateIndex()
    {
        // Created on table creation
    }


    public override bool WriteJson(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            var test = new PersistenceTestModel
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            };

            using var cmd = new NpgsqlCommand("INSERT INTO json_data (id,json) VALUES(@id,@json)", lease.Connection);
            cmd.Parameters.AddWithValue("id", test.Id);
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

    public override bool ReadJson(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"SELECT * FROM json_data WHERE id = @id", lease.Connection);
            cmd.Parameters.AddWithValue("@id", i);

            using var reader = cmd.ExecuteReader(); // nonasync is slower
            reader.Read();
            var json = JsonSerializer.Deserialize<PersistenceTestModel>((string)reader["json"]);
        }
        catch (Exception ex)
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
            await using (var cmd = lease.DataSource.CreateCommand(@"INSERT INTO 
                    country_postal(id,country_code,postal_code,place_name,latitude,longitude) 
                    VALUES($1,$2,$3,$4,$5,$6)"))
            {
                cmd.Parameters.Add(new() { Value = message.Id });
                cmd.Parameters.Add(new() { Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.Latitude == null ? DBNull.Value : message.Latitude, NpgsqlDbType = NpgsqlDbType.Double });
                cmd.Parameters.Add(new() { Value = message.Longitude == null ? DBNull.Value : message.Longitude, NpgsqlDbType = NpgsqlDbType.Double });
                cmd.ExecuteNonQuery();
            }
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
        bool result = true;
        var lease = Pool.Get();

        try
        {
            await using (var cmd = lease.DataSource.CreateCommand(@"UPDATE country_postal SET country_code = $2, postal_code = $3, place_name = $4, latitude = $5, longitude = $6 WHERE id = $1"))
            {
                cmd.Parameters.Add(new() { Value = message.Id });
                cmd.Parameters.Add(new() { Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, NpgsqlDbType = NpgsqlDbType.Varchar });
                cmd.Parameters.Add(new() { Value = message.Latitude == null ? DBNull.Value : message.Latitude, NpgsqlDbType = NpgsqlDbType.Double });
                cmd.Parameters.Add(new() { Value = message.Longitude == null ? DBNull.Value : message.Longitude, NpgsqlDbType = NpgsqlDbType.Double });
                cmd.ExecuteNonQuery();
            }
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
            await using (var cmd = lease.DataSource.CreateCommand(@"SELECT * FROM country_postal WHERE id = $1"))
            {
                cmd.Parameters.Add(new() { Value = (long)message.Id, NpgsqlDbType = NpgsqlDbType.Bigint });
                using var reader = cmd.ExecuteReader();
                reader.Read();

                var match = CountryPostalCode.GetFromReader(reader);
            }
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
            await using (var cmd = lease.DataSource.CreateCommand(@"SELECT * FROM country_postal WHERE postal_code = $1"))
            {
                cmd.Parameters.Add(new() { Value = message.PostalCode, NpgsqlDbType = NpgsqlDbType.Text });
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var match = CountryPostalCode.GetFromReader(reader);
                }
            }
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

            CountryPostalCode match = null;
            await using (var cmd = lease.DataSource.CreateCommand(@"SELECT * FROM country_postal WHERE id = $1"))
            {
                cmd.Parameters.Add(new() { Value = (long)message.Id, NpgsqlDbType = NpgsqlDbType.Bigint });
                using var reader = cmd.ExecuteReader();
                reader.Read();

                match = CountryPostalCode.GetFromReader(reader);


            }

            await using (var cmd2 = lease.DataSource.CreateCommand(@"SELECT * FROM country_postal WHERE postal_code = $1"))
            {
                cmd2.Parameters.Add(new() { Value = match.PostalCode, NpgsqlDbType = NpgsqlDbType.Text });
                using var reader2 = cmd2.ExecuteReader();

                while (reader2.Read())
                {
                    var match2 = CountryPostalCode.GetFromReader(reader2);
                }
            }

            //await using (var cmd = lease.DataSource.CreateCommand(@"SELECT c2.* FROM country_postal c
            //            LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
            //        WHERE c.id = $1 AND c.id != c2.id"))
            //{
            //    cmd.Parameters.Add(new() { Value = (long)message.Id, NpgsqlDbType = NpgsqlDbType.Bigint });
            //    using var reader = cmd.ExecuteReader();

            //    while (reader.Read())
            //    {
            //        var match = CountryPostalCode.GetFromReader(reader);
            //    }
            //}
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}