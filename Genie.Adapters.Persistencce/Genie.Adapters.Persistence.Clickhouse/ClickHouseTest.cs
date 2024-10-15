using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Genie.Core.Persistence;
using ClickHouse.Client.Utility;
using static System.Net.Mime.MediaTypeNames;
using Humanizer;


namespace Genie.Adapters.Persistence.ClickHouse;

public class ClickHouseTest(int payload, ObjectPool<ClickHousePooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public ObjectPool<ClickHousePooledObject> Pool = pool;

    public int Payload { get; set; } = payload;

    public ClickHouseTest() : this(JsonSize,new DefaultObjectPool<ClickHousePooledObject>(new DefaultPooledObjectPolicy<ClickHousePooledObject>()))
    {

    }

    public ClickHouseTest(bool create) : this(JsonSize,new DefaultObjectPool<ClickHousePooledObject>(new DefaultPooledObjectPolicy<ClickHousePooledObject>()))
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

        lease.Connection.ExecuteStatementAsync("ALTER TABLE country_data ADD INDEX postal_idx lowerUTF8(postal_code) TYPE tokenbf_v1(10240, 3, 0) GRANULARITY 4").GetAwaiter().GetResult();

        Pool.Return(lease);
    }


    public void CreateJsonDB()
    {
        var lease = Pool.Get();

        lease.Connection.ExecuteStatementAsync(@"CREATE OR REPLACE TABLE json_data (
            id text PRIMARY KEY,
            json text,
            last_update_timestamp timestamp
        );").GetAwaiter().GetResult();


        Pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        lease.Connection.ExecuteStatementAsync(@"CREATE OR REPLACE TABLE country_data (
                id Int64 PRIMARY KEY,
                country_code String,
                postal_code String,
                place_name String,
                latitude Float64 CODEC(LZ4HC(9)),
                longitude Float64 CODEC(LZ4HC(9)),
            );").GetAwaiter().GetResult();

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


            using var cmd = lease.Connection.CreateCommand();
            cmd.CommandText = "INSERT INTO json_data(id, json) VALUES ({id:String}, {json:String});";
            cmd.AddParameter("id", i.ToString());
            cmd.AddParameter("json", JsonSerializer.Serialize(test));
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
            using var cmd = lease.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM json_data WHERE id = {id:String}";
            cmd.AddParameter("id", i.ToString());

            using var reader = cmd.ExecuteReader();
            reader.Read();

            var result = JsonSerializer.Deserialize<PersistenceTestModel>((string)reader["json"]);

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

            using var cmd = lease.Connection.CreateCommand();
            cmd.CommandText = "INSERT INTO country_data(id, country_code, postal_code, place_name, latitude, longitude) VALUES ({id:Int64}, {country_code:String}, {postal_code:String}, {place_name:String}, {latitude:Float64}, {longitude:Float64})";
            cmd.AddParameter("id", (long)message.Id);
            cmd.AddParameter("country_code", message.CountryCode == null ? DBNull.Value : message.CountryCode);
            cmd.AddParameter("postal_code", message.PostalCode == null ? DBNull.Value : message.PostalCode);
            cmd.AddParameter("place_name", message.PlaceName == null ? DBNull.Value : message.PlaceName);
            cmd.AddParameter("latitude", message.Latitude == null ? DBNull.Value : message.Latitude);
            cmd.AddParameter("longitude", message.Longitude == null ? DBNull.Value : message.Longitude);

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

            using var cmd = lease.Connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE country_data UPDATE country_code = {country_code:String}, postal_code = {postal_code:String}, place_name = {place_name:String}, latitude = {latitude:Float64}, longitude = {longitude:Float64} WHERE id = {id:Int64}";
            cmd.AddParameter("id", (long)message.Id);
            cmd.AddParameter("country_code", message.CountryCode == null ? DBNull.Value : message.CountryCode);
            cmd.AddParameter("postal_code", message.PostalCode == null ? DBNull.Value : message.PostalCode);
            cmd.AddParameter("place_name", message.PlaceName == null ? DBNull.Value : message.PlaceName);
            cmd.AddParameter("latitude", message.Latitude == null ? DBNull.Value : message.Latitude);
            cmd.AddParameter("longitude", message.Longitude == null ? DBNull.Value : message.Longitude);

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
            using var cmd = lease.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM country_data WHERE id = {id:Int64}";
            cmd.AddParameter("id", (long)message.Id);

            using var reader = cmd.ExecuteReader();
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
            using var cmd = lease.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM country_data WHERE postal_code = {postal_code:String}";
            cmd.AddParameter("postal_code", message.PostalCode);

            using var reader = cmd.ExecuteReader();

            while(reader.Read())
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
            // ClickHouse throws an exception if trying to return the secondary table so this
            // query has been reversed
            using var cmd = lease.Connection.CreateCommand();
            cmd.CommandText = @"SELECT c.* FROM country_data c
                LEFT JOIN country_data c2 ON c.postal_code = c2.postal_code
                        WHERE c.id != c2.id AND c2.id = {id:Int64}";
            cmd.AddParameter("id", (long)message.Id);

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