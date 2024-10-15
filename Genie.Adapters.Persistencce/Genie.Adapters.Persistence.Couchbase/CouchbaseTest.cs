using Confluent.Kafka;
using Couchbase.KeyValue;
using Couchbase.Management.Search;
using Couchbase.Protostellar.Query.V1;
using CouchDB.Driver.Indexes;
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace Genie.Adapters.Persistence.Couchbase;


public class CouchbaseTest(int payload, ObjectPool<CouchbasePooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;

    public ObjectPool<CouchbasePooledObject> Pool = pool;

    public const int Count = 250000;

    public CouchbaseTest() : this(JsonSize,new DefaultObjectPool<CouchbasePooledObject>(new DefaultPooledObjectPolicy<CouchbasePooledObject>()))
    {

    }

    public CouchbaseTest(bool create) : this(JsonSize,new DefaultObjectPool<CouchbasePooledObject>(new DefaultPooledObjectPolicy<CouchbasePooledObject>()))
    {
        if (create)
            CreateDB();
    }

    public override void CreateDB()
    {
        CreateIndex();
    }

    public override void CreateIndex()
    {
        var lease = Pool.Get();

        lease.Scope.QueryAsync<dynamic>("CREATE INDEX adv_postalCode ON `default`:`genie`.`genie_scope`.`country_postal`(`postalCode`)").GetAwaiter().GetResult();
        
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

            var result = lease.Json.UpsertAsync(test.Id, test).GetAwaiter().GetResult();

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
            var other = lease.Json.GetAsync(i.ToString()).GetAwaiter().GetResult();
            var match = other.ContentAs<PersistenceTestModel>();
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
            var response = await lease.Postal.InsertAsync(message.Id.ToString(), message);
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
            var response = await lease.Postal.UpsertAsync(message.Id.ToString(), message);
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
            var other = await lease.Postal.GetAsync(message.Id.ToString());
            var postal = other.ContentAs<CountryPostalCode>();
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
            var query = await lease.Scope.QueryAsync<JToken>($@"SELECT * FROM country_postal WHERE postalCode = '{message.PostalCode}';");
            var list = await query.ToListAsync();

            foreach (var item in list)
            {
                var postal = JsonSerializer.Deserialize<CountryPostalCode>(item.First!.First!.ToString(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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
            var other = await lease.Postal.GetAsync(message.Id.ToString());
            var match = other.ContentAs<CountryPostalCode>();


            var query = await lease.Scope.QueryAsync<JToken>($@"SELECT * FROM country_postal WHERE postalCode = '{match!.PostalCode}' AND id != {message.Id};");
            var list = await query.ToListAsync();

            foreach (var item in list)
            {
                var postal = JsonSerializer.Deserialize<CountryPostalCode>(item.First!.First!.ToString(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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