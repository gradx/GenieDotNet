
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using SingleStoreConnector;
using System.Text.Json;

namespace Genie.Adapters.Persistence.SingleStore;

public class SingleStoreTest(int payload, ObjectPool<SingleStorePooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    readonly ObjectPool<SingleStorePooledObject> Pool = pool;

    public SingleStoreTest() : this(JsonSize,new DefaultObjectPool<SingleStorePooledObject>(new DefaultPooledObjectPolicy<SingleStorePooledObject>()))
    {

    }

    public SingleStoreTest(bool create) : this(JsonSize,new DefaultObjectPool<SingleStorePooledObject>(new DefaultPooledObjectPolicy<SingleStorePooledObject>()))
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

        using var cmd = new SingleStoreCommand("CREATE INDEX idx_postal ON country_postal(postal_code)", lease.Connection);
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void CreateJsonDB()
    {
        var lease = Pool.Get();

        using var cmd = new SingleStoreCommand("DROP TABLE IF EXISTS json_data", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE TABLE json_data(id VARCHAR(25) NOT NULL, json JSON NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))";
        cmd.ExecuteNonQuery();

        Pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        using var cmd = new SingleStoreCommand(@"DROP TABLE IF EXISTS country_postal;", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE country_postal (
                  id BIGINT PRIMARY KEY,
                  country_code VARCHAR (255), 
                  postal_code VARCHAR (255),
                  place_name VARCHAR (255), 
                  latitude double,
                  longitude double
                );";

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

            using var cmd = new SingleStoreCommand("INSERT INTO json_data (id,json) VALUES(@id, @json)", lease.Connection);
            cmd.Parameters.Add(new() { ParameterName = "id", Value = test.Id });
            cmd.Parameters.Add(new() { ParameterName = "json", Value = JsonSerializer.Serialize(test), SingleStoreDbType = SingleStoreDbType.LongText });
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
            using var cmd = new SingleStoreCommand(@"SELECT * FROM json_data WHERE id = @id", lease.Connection);
            cmd.Parameters.Add(new() { ParameterName = "id", Value = i });

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
            using var cmd = new SingleStoreCommand(@"INSERT INTO country_postal(id,country_code,postal_code,place_name,latitude,longitude) VALUES(@id,@country_code,@postal_code,@place_name,@latitude,@longitude)", lease.Connection);
            cmd.Parameters.Add(new() { ParameterName = "id", Value = message.Id });
            cmd.Parameters.Add(new() { ParameterName = "country_code", Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "place_name", Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "latitude", Value = message.Latitude == null ? DBNull.Value : message.Latitude, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "longitude", Value = message.Longitude == null ? DBNull.Value : message.Longitude, SingleStoreDbType = SingleStoreDbType.TinyText });
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
            using var cmd = new SingleStoreCommand(@"UPDATE country_postal SET country_code = @country_code, postal_code = @postal_code, place_name = @place_name,
                        latitude = @latitude, longitude = @longitude WHERE id = @id", lease.Connection);
            cmd.Parameters.Add(new() { ParameterName = "id", Value = message.Id });
            cmd.Parameters.Add(new() { ParameterName = "country_code", Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "place_name", Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "latitude", Value = message.Latitude == null ? DBNull.Value : message.Latitude, SingleStoreDbType = SingleStoreDbType.TinyText });
            cmd.Parameters.Add(new() { ParameterName = "longitude", Value = message.Longitude == null ? DBNull.Value : message.Longitude, SingleStoreDbType = SingleStoreDbType.TinyText });
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
            using var cmd = new SingleStoreCommand(@"SELECT * FROM country_postal WHERE id = @id", lease.Connection); ;
            cmd.Parameters.Add(new() { ParameterName = "id", Value = (long)message.Id, SingleStoreDbType = SingleStoreDbType.Int64 });

            using var reader = cmd.ExecuteReader();
            reader.Read();

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
            using var cmd = new SingleStoreCommand(@"SELECT * FROM country_postal WHERE postal_code = @postal_code", lease.Connection); ;
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode, SingleStoreDbType = SingleStoreDbType.Int64 });

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
            using var cmd = new SingleStoreCommand(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.id = @id AND c.id != c2.id", lease.Connection); ;

            cmd.Parameters.Add(new() { ParameterName = "id", Value = (long)message.Id, SingleStoreDbType = SingleStoreDbType.Int64 });
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