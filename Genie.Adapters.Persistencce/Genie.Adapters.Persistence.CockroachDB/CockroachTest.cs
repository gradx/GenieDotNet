using Confluent.Kafka;
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Genie.Adapters.Persistence.CockroachDB;

public class CockroachTest(int payload, ObjectPool<CockroachPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    public ObjectPool<CockroachPooledObject> Pool = pool;

    public CockroachTest() : this(JsonSize,new DefaultObjectPool<CockroachPooledObject>(new DefaultPooledObjectPolicy<CockroachPooledObject>()))
    {

    }
    public CockroachTest(bool create) : this(JsonSize,new DefaultObjectPool<CockroachPooledObject>(new DefaultPooledObjectPolicy<CockroachPooledObject>()))
    {
        if (create)
            CreateDB();
    }


    public override void CreateDB()
    {
        CreateJsonDB();
        CreatePostalDB();
    }

    public override void CreateIndex()
    {
        var lease = Pool.Get();

        using var cmd = new NpgsqlCommand(@"CREATE INDEX postal_idx ON country_postal(postal_code);", lease.Connection);
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void CreateJsonDB()
    {
        var lease = Pool.Get();
        using var cmd = new NpgsqlCommand(@"DROP TABLE IF EXISTS json_data", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE json_data(id text NOT NULL, 
                json JSONB NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))";


        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }
    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        using var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS country_postal; CREATE TABLE country_postal(id INT PRIMARY KEY, country_code STRING, postal_code STRING, place_name STRING, latitude double precision, longitude double precision);", lease.Connection);
        cmd.ExecuteNonQuery();

        CreateIndex();

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
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"SELECT * FROM json_data WHERE id = @id", lease.Connection);
            cmd.Parameters.AddWithValue("@id",i);

            using var reader = cmd.ExecuteReader(); // nonasync is slower
            reader.Read();
            var json = JsonSerializer.Deserialize<PersistenceTestModel>((string)reader["json"]);

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
            using var cmd = new NpgsqlCommand(@"INSERT INTO country_postal (id, country_code, postal_code, place_name, latitude, longitude) VALUES  (@id, @country_code, @postal_code, @place_name, @latitude, @longitude)", lease.Connection);
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

    public override async Task<bool> UpdatePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"UPDATE country_postal SET country_code = @country_code, postal_code = @postal_code, place_name = @place_name, latitude = @latitude, longitude = @longitude WHERE id = @id", lease.Connection);
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

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new NpgsqlCommand(@"SELECT * FROM country_postal WHERE id = @id", lease.Connection);

            cmd.Parameters.AddWithValue("@id", message.Id);

            using var reader = cmd.ExecuteReader(); // nonasync is slower
            reader.Read();
            var json = CountryPostalCode.GetFromReader(reader);
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
            using var cmd = new NpgsqlCommand(@"SELECT * FROM country_postal WHERE postal_code = @postal_code", lease.Connection);
            cmd.Parameters.AddWithValue("@postal_code", message.PostalCode);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var json = CountryPostalCode.GetFromReader(reader);
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
            using var cmd = new NpgsqlCommand(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.id = @id AND c.id != c2.id", lease.Connection);
            cmd.Parameters.AddWithValue("@id", message.Id);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var json = CountryPostalCode.GetFromReader(reader);
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