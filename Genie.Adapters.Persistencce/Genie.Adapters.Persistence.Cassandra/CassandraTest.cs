using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Genie.Core.Persistence;
using Cassandra;

namespace Genie.Adapters.Persistence.Cassandra;

public class CassandraTest(int payload, ObjectPool<CassandraPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public ObjectPool<CassandraPooledObject> Pool = pool;

    public int Payload { get; set; } = payload;

    public CassandraTest() : this(JsonSize, new DefaultObjectPool<CassandraPooledObject>(new DefaultPooledObjectPolicy<CassandraPooledObject>()))
    {

    }

    public CassandraTest(bool create) : this(JsonSize, new DefaultObjectPool<CassandraPooledObject>(new DefaultPooledObjectPolicy<CassandraPooledObject>()))
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

        _ = lease.Session.Execute(@"CREATE INDEX IF NOT EXISTS postal_idx ON genie.country_data (postal_code)");

        Pool.Return(lease);
    }

    public void CreateJsonDB()
    {
        var lease = Pool.Get();

        _ = lease.Session.Execute("DROP TABLE IF EXISTS genie.json_data;");

        _ = lease.Session.Execute(@"CREATE TABLE IF NOT EXISTS genie.json_data (
            id text PRIMARY KEY,
            json text
            );");

        Pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = Pool.Get();

        _ = lease.Session.Execute(@"DROP TABLE IF EXISTS genie.country_data;");

        _ = lease.Session.Execute(@"CREATE TABLE IF NOT EXISTS genie.country_data (
                    id bigint PRIMARY KEY,
                    country_code text,
                    postal_code text,
                    place_name text,
                    latitude double,
                    longitude double,
                );"
        );

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


            var sql = $@"INSERT INTO genie.json_data(id, json) VALUES (?, ?)";
            var prepared = lease.Session.Prepare(sql);
            lease.Session.Execute(prepared.Bind(test.Id, JsonSerializer.Serialize(test)));
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
            var test = new PersistenceTestModel
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            };

            var sql = $@"SELECT * FROM genie.json_data WHERE id = ?";
            var prepared = lease.Session.Prepare(sql);
            lease.Session.Execute(prepared.Bind(test.Id));
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
            var sql = $@"INSERT INTO genie.country_data(id, country_code, postal_code, place_name, latitude, longitude) VALUES (?, ?, ?, ?, ?, ?)";
            var prepared = lease.Session.Prepare(sql);
            lease.Session.Execute(prepared.Bind((long)message.Id, message.CountryCode, message.PostalCode, message.PlaceName, message.Latitude, message.Longitude));
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
            var sql = $@"UPDATE genie.country_data SET country_code = ?, postal_code = ?, place_name = ?, latitude = ?, longitude = ? WHERE id = ?";
            var prepared = lease.Session.Prepare(sql);
            lease.Session.Execute(prepared.Bind(message.CountryCode, message.PostalCode, message.PlaceName, message.Latitude, message.Longitude, (long)message.Id));
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
            var read = lease.Session.Prepare("SELECT * FROM genie.country_data WHERE id = ?");
            var match = lease.Session.Execute(read.Bind((long)message.Id));
            var first = match.FirstOrDefault()!;

            var countryCode = GetCode((long)first["id"], first);
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
            var sql = $@"SELECT * FROM genie.country_data WHERE postal_code = ? ALLOW FILTERING";
            var read = lease.Session.Prepare(sql);
            var response = lease.Session.Execute(read.Bind(message.PostalCode));
            var matches = response.ToList();

            foreach(var match in matches)
            {
                var countryCode = GetCode((long)match["id"], match);
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
            var read_id = lease.Session.Prepare("SELECT * FROM genie.country_data WHERE id = ?");
            var id_result = lease.Session.Execute(read_id.Bind((long)message.Id));
            var first = id_result.FirstOrDefault()!;


            var sql = $@"SELECT * FROM genie.country_data WHERE postal_code = ? ALLOW FILTERING";
            var postal_statement = lease.Session.Prepare(sql);
            var postal_response = lease.Session.Execute(postal_statement.Bind((string)first["postal_code"]));

            var postal_results = postal_response.ToList();

            foreach (var match in postal_results)
            {
                var countryCode = GetCode((long)first["id"], match);
            }
        }
        catch (Exception ex)
        {
            result = false;
        }


        Pool.Return(lease);
        return result;
    }

    private CountryPostalCode GetCode(long id, Row match)
    {
        return new CountryPostalCode
        {
            Id = id,
            CountryCode = match["country_code"] is DBNull ? null : (string)match["country_code"],
            PostalCode = match["postal_code"] is DBNull ? null : (string)match["postal_code"],
            PlaceName = match["place_name"] is DBNull ? null : (string)match["place_name"],
            Latitude = match["latitude"] is DBNull ? null : (double)match["latitude"],
            Longitude = match["longitude"] is DBNull ? null : (double)match["longitude"]
        };
    }
}