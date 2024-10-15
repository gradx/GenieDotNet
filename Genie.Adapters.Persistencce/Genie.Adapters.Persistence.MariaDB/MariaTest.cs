using Confluent.Kafka;
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using MySqlConnector;
using System.Text.Json;

namespace Genie.Adapters.Persistence.MariaDB;

public class MariaTest(int payload, ObjectPool<MariaPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MariaPooledObject> Pool = pool;

    public MariaTest() : this(JsonSize,new DefaultObjectPool<MariaPooledObject>(new DefaultPooledObjectPolicy<MariaPooledObject>()))
    {

    }

    public MariaTest(bool create) : this(JsonSize,new DefaultObjectPool<MariaPooledObject>(new DefaultPooledObjectPolicy<MariaPooledObject>()))
    {
        if (create)
            CreateDB();
    }


    public override void CreateDB()
    {
        CreateMariaJsonDB();
        CreatePostalDB();
    }

    public void CreateMariaJsonDB()
    {
        var lease = Pool.Get();

        using MySqlCommand cmd = new(@"DROP TABLE IF EXISTS json_data; 
            CREATE TABLE json_data (id VARCHAR(255) PRIMARY KEY, json LONGTEXT);", lease.Connection);

        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void CreateMySqlDB()
    {
        CreateMySqlJsonDB();
        CreatePostalDB();
    }

    private void CreateMySqlJsonDB()
    {
        var lease = Pool.Get();

        using MySqlCommand cmd = new(@"DROP TABLE IF EXISTS json_data; 
            CREATE TABLE json_data (id VARCHAR(255) PRIMARY KEY, json JSON);", lease.Connection);

        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        using MySqlCommand cmd = new(@"DROP TABLE IF EXISTS country_postal; 
                CREATE TABLE country_postal (id BIGINT PRIMARY KEY, country_code TINYTEXT, postal_code VARCHAR(255), place_name TINYTEXT, latitude DOUBLE, longitude DOUBLE)", lease.Connection);

        cmd.ExecuteNonQuery();

        CreateIndex();

        Pool.Return(lease);
    }

    public override void CreateIndex()
    {
        var lease = Pool.Get();

        using MySqlCommand cmd = new(@"CREATE INDEX idx_postal_code ON country_postal(postal_code);", lease.Connection);

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

            using MySqlCommand cmd = new("INSERT INTO json_data (id, json) VALUES (@id, @json) ON DUPLICATE KEY UPDATE json=@json", lease.Connection);
            cmd.Parameters.AddWithValue("@id", i.ToString());
            cmd.Parameters.AddWithValue("@json", JsonSerializer.Serialize(test));
            cmd.ExecuteNonQuery();
        }
        catch(Exception ex)
        {

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
            using MySqlCommand cmd = new(@"SELECT * FROM json_data WHERE id = @id;", lease.Connection);
            cmd.Parameters.AddWithValue("@id",i.ToString());
            using var reader = cmd.ExecuteReader();

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
            using MySqlCommand cmd = new(@"INSERT INTO country_postal (id, country_code, postal_code, place_name, 
                        latitude, longitude) VALUES (@id, @country_code, @postal_code, @place_name, @latitude, @longitude)", lease.Connection);
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


    public override async Task<bool> UpdatePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            using MySqlCommand cmd = new(@"UPDATE country_postal SET country_code = @country_code, postal_code = @postal_code, place_name = @place_name,
                        latitude = @latitude, longitude = @longitude WHERE id = @id;", lease.Connection);
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
            using MySqlCommand cmd = new(@"SELECT * FROM country_postal WHERE id = @id;", lease.Connection);
            cmd.Parameters.AddWithValue("@id", message.Id);
            using var reader = cmd.ExecuteReader();

            reader.Read();
            var cc = CountryPostalCode.GetFromReader(reader);

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
            using MySqlCommand cmd = new(@"SELECT * FROM country_postal WHERE postal_code = @postal_code;", lease.Connection);
            cmd.Parameters.AddWithValue("@postal_code", message.PostalCode);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var cc = CountryPostalCode.GetFromReader(reader);
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
            using MySqlCommand cmd = new(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.id = @id AND c.id != c2.id;", lease.Connection);
            cmd.Parameters.AddWithValue("@id", message.Id);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var cc = CountryPostalCode.GetFromReader(reader);
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