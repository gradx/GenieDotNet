
using Genie.Core.Persistence;
using IBM.Data.Db2;
using Microsoft.Extensions.ObjectPool;
using SpanJson.Formatters;
using System.Text.Json;

namespace Genie.Adapters.Persistence.DB2;

public class DB2Test(int payload, ObjectPool<DB2PooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    readonly ObjectPool<DB2PooledObject> Pool = pool;

    public DB2Test() : this(JsonSize,new DefaultObjectPool<DB2PooledObject>(new DefaultPooledObjectPolicy<DB2PooledObject>()))
    {

    }
    public DB2Test(bool create) : this(JsonSize,new DefaultObjectPool<DB2PooledObject>(new DefaultPooledObjectPolicy<DB2PooledObject>()))
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

        using var cmd = new DB2Command("DROP TABLE IF EXISTS json_data", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE TABLE json_data (id VARCHAR (25) NOT NULL, json CLOB NOT NULL, CONSTRAINT PK_bench PRIMARY KEY (id))";
        cmd.ExecuteNonQuery();
        
        Pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        using var cmd = new DB2Command(@"DROP TABLE IF EXISTS country_postal;", lease.Connection);
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE country_postal (
                  id BIGINT NOT NULL,
                  country_code VARCHAR (255), 
                  postal_code VARCHAR (255),
                  place_name VARCHAR(255), 
                  latitude double precision,
                  longitude double precision,
                  CONSTRAINT PK_bench PRIMARY KEY (id)
                );";

        cmd.ExecuteNonQuery();

        CreateIndex();

        Pool.Return(lease);
    }

    public override void CreateIndex()
    {
        var lease = Pool.Get();

        using var cmd = new DB2Command(@"CREATE INDEX idx_postal ON country_postal(postal_code);", lease.Connection);
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

            //using var cmd = new DB2Command(@"MERGE INTO json_data AS mt USING (
            //        SELECT * FROM TABLE (
            //            VALUES 
            //                (@id, @json)
            //        )
            //    ) AS vt(id, json) ON (mt.id = vt.id)
            //    WHEN MATCHED THEN
            //        UPDATE SET json = vt.json
            //    WHEN NOT MATCHED THEN
            //        INSERT (id, json) VALUES (vt.id, vt.json)
            //    ;", lease.Connection);

            using var cmd = new DB2Command("INSERT INTO json_data(id, json) VALUES (@id, @json)", lease.Connection);
            cmd.Parameters.Add("id", test.Id);
            cmd.Parameters.Add("json", JsonSerializer.Serialize(test));
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

    public override bool ReadJson(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            using var cmd = new DB2Command(@"SELECT * FROM json_data WHERE id = @id", lease.Connection);
            cmd.Parameters.Add("id", i.ToString());
            
            using var reader = cmd.ExecuteReader();
            var found = reader.Read();
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
            //using var cmd = new DB2Command(@"MERGE INTO country_postal AS mt USING (
            //        SELECT * FROM TABLE (
            //            VALUES 
            //                (@id, @country_code, @postal_code, @place_name, @latitude, @longitude)
            //        )
            //    ) AS vt(id, country_code, postal_code, place_name, latitude, longitude) ON (mt.id = vt.id)
            //    WHEN MATCHED THEN
            //        UPDATE SET country_code = vt.country_code, postal_code = vt.postal_code, place_name = vt.place_name, latitude = vt.latitude, longitude = vt.longitude
            //    WHEN NOT MATCHED THEN
            //        INSERT (id, country_code, postal_code, place_name, latitude, longitude) VALUES (vt.id, vt.country_code, vt.postal_code, vt.place_name, vt.latitude, vt.longitude)
            //    ;", lease.Connection);


            using var cmd = new DB2Command("INSERT INTO country_postal(id, country_code, postal_code, place_name, latitude, longitude) VALUES (@id, @country_code, @postal_code, @place_name, @latitude, @longitude)", lease.Connection);
            cmd.Parameters.Add(new() { ParameterName = "id", Value = message.Id });
            cmd.Parameters.Add(new() { ParameterName = "country_code", Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "place_name", Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "latitude", Value = message.Latitude == null ? DBNull.Value : message.Latitude, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "longitude", Value = message.Longitude == null ? DBNull.Value : message.Longitude, DB2Type = DB2Type.VarChar });

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
            using var cmd = new DB2Command(@"MERGE INTO country_postal AS mt USING (
                    SELECT * FROM TABLE (
                        VALUES 
                            (@id, @country_code, @postal_code, @place_name, @latitude, @longitude)
                    )
                ) AS vt(id, country_code, postal_code, place_name, latitude, longitude) ON (mt.id = vt.id)
                WHEN MATCHED THEN
                    UPDATE SET country_code = vt.country_code, postal_code = vt.postal_code, place_name = vt.place_name, latitude = vt.latitude, longitude = vt.longitude
                WHEN NOT MATCHED THEN
                    INSERT (id, country_code, postal_code, place_name, latitude, longitude) VALUES (vt.id, vt.country_code, vt.postal_code, vt.place_name, vt.latitude, vt.longitude)
                ;", lease.Connection);

            cmd.Parameters.Add(new() { ParameterName = "id", Value = message.Id });
            cmd.Parameters.Add(new() { ParameterName = "country_code", Value = message.CountryCode == null ? DBNull.Value : message.CountryCode, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "postal_code", Value = message.PostalCode == null ? DBNull.Value : message.PostalCode, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "place_name", Value = message.PlaceName == null ? DBNull.Value : message.PlaceName, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "latitude", Value = message.Latitude == null ? DBNull.Value : message.Latitude, DB2Type = DB2Type.VarChar });
            cmd.Parameters.Add(new() { ParameterName = "longitude", Value = message.Longitude == null ? DBNull.Value : message.Longitude, DB2Type = DB2Type.VarChar });

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
            using var cmd = new DB2Command(@"SELECT * FROM country_postal WHERE id = @id", lease.Connection);
            cmd.Parameters.Add("id", message.Id);

            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

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
            using var cmd = new DB2Command(@"SELECT * FROM country_postal WHERE postal_code = @postal_code", lease.Connection);
            cmd.Parameters.Add("postal_code", message.PostalCode);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
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
            using var cmd = new DB2Command(@"SELECT c2.* FROM country_postal c
                        LEFT JOIN country_postal c2 ON c.postal_code = c2.postal_code 
                    WHERE c.id = @id AND c.id != c2.id", lease.Connection); ;

            cmd.Parameters.Add("id", message.Id);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
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