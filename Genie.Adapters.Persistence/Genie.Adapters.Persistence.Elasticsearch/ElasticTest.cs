
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.Elasticsearch;

public class ElasticTest(int payload, ObjectPool<ElasticsearchPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<ElasticsearchPooledObject> Pool = pool;
    

    public static void CreateDB()
    {

    }

    public void CreateIndex()
    {

    }

    public override bool WriteJson(long i)
    {
        bool success = true;
        var lease = Pool.Get();

        try
        {
            var test = new PersistenceTestModel
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };

            var result = lease.Client.IndexAsync(test).GetAwaiter().GetResult();
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
        return true;
    }


    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            var match = await lease.Client.IndexAsync(message);
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
            var match = await lease.Client.GetAsync<CountryPostalCode>("genie", $@"{message.Id}");
            var cc = match.Source;
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
            var response = await lease.Client.SearchAsync<CountryPostalCode>(s => s
                .Index("genie")
                .Query(q => q
                    .Term(new TermQuery(new Field("postalCode")) { Value = message.PostalCode })
                ));

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
            var match = await lease.Client.GetAsync<CountryPostalCode>("genie", $@"{message.Id}");
            var cc = match.Source;

            var response = await lease.Client.SearchAsync<CountryPostalCode>(s => s
                .Index("genie")
                .Query(q => q
                    .Term(new TermQuery(new Field("postalCode")) { Value = cc.PostalCode })
                ));
        }
        catch (Exception ex)
        {
            result = false;
        }

        Pool.Return(lease);
        return result;
    }
}