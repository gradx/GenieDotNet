
using Genie.Core.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Identity.Client;
using System.Data;
using System.Text.Json;

namespace Genie.Adapters.Persistence.SqlServer;

public class SqlServerTest(int payload, ObjectPool<SqlServerPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    readonly ObjectPool<SqlServerPooledObject> Pool = pool;

    public SqlServerTest() : this(JsonSize,new DefaultObjectPool<SqlServerPooledObject>(new DefaultPooledObjectPolicy<SqlServerPooledObject>()))
    {

    }

    public SqlServerTest(bool create) : this(JsonSize,new DefaultObjectPool<SqlServerPooledObject>(new DefaultPooledObjectPolicy<SqlServerPooledObject>()))
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

        using var cmd = new SqlCommand("DROP TABLE IF EXISTS json_data", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE TABLE json_data(id VARCHAR(25) NOT NULL, json TEXT NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))";
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        using var cmd = new SqlCommand(@"DROP TABLE IF EXISTS country_postal;", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE country_postal (
                  id BIGINT,
                  country_code VARCHAR(255),
                  postal_code VARCHAR(255),
                  place_name VARCHAR(255), 
                  latitude float,
                  longitude float,
                  CONSTRAINT PK_postal PRIMARY KEY (id)
                )";

        cmd.ExecuteNonQuery();

        CreateIndex();

        Pool.Return(lease);
    }

    public override void CreateIndex()
    {
        var lease = Pool.Get();

        using var cmd = new SqlCommand("CREATE INDEX idx_postal ON country_postal(postal_code)", lease.Connection);
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
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

            using var cmd = new SqlCommand("INSERT INTO json_data (id,json) VALUES(@id, @json)", lease.Connection);
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
            using var cmd = new SqlCommand(@"SELECT * FROM json_data WHERE id = @id", lease.Connection);
            cmd.Parameters.AddWithValue("id", i);
            
            using var reader = cmd.ExecuteReader();
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
            using var cmd = new SqlCommand(@"INSERT INTO country_postal(id, country_code, postal_code, place_name, latitude, longitude) 
                VALUES(@id, @country_code, @postal_code, @place_name, @latitude, @longitude)", lease.Connection);
            cmd.Parameters.Add(new() { ParameterName = "id", Value = message.Id });
            cmd.Parameters.Add(new() { ParameterName = "country_code", Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "place_name", Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "latitude", Value = message.Latitude == null ? DBNull.Value : message.Latitude, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "longitude", Value = message.Longitude == null ? DBNull.Value : message.Longitude, SqlDbType = SqlDbType.VarChar });
            cmd.ExecuteNonQuery();
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
            using var cmd = new SqlCommand(@"UPDATE country_postal SET country_code = @country_code, postal_code = @postal_code, place_name = @place_name,
                        latitude = @latitude, longitude = @longitude WHERE id = @id", lease.Connection);
            cmd.Parameters.Add(new() { ParameterName = "id", Value = message.Id });
            cmd.Parameters.Add(new() { ParameterName = "country_code", Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "place_name", Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "latitude", Value = message.Latitude == null ? DBNull.Value : message.Latitude, SqlDbType = SqlDbType.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "longitude", Value = message.Longitude == null ? DBNull.Value : message.Longitude, SqlDbType = SqlDbType.VarChar });
            cmd.ExecuteNonQuery();
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
            using var cmd = new SqlCommand(@"SELECT * FROM country_postal WHERE id = @id", lease.Connection); ;
            cmd.Parameters.Add(new() { ParameterName = "id", Value = (long)message.Id, SqlDbType = SqlDbType.BigInt });

            using var reader = cmd.ExecuteReader();
            reader.ReadAsync();

            var match = CountryPostalCode.GetFromReader(reader);
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
            using var cmd = new SqlCommand(@"SELECT * FROM country_postal WHERE postal_code = @postal_code", lease.Connection); ;
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode, SqlDbType = SqlDbType.VarChar });

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
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

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new SqlCommand(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.id = @id AND c.id != c2.id", lease.Connection); ;

            cmd.Parameters.Add(new() { ParameterName = "id", Value = (long)message.Id, SqlDbType = SqlDbType.BigInt });
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
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
}