
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.ObjectPool;
using Genie.Core.Persistence;
using Confluent.Kafka;
using Elastic.Transport;

namespace Genie.Adapters.Persistence.Elasticsearch;

public class ElasticTest(int payload, ObjectPool<ElasticsearchPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<ElasticsearchPooledObject> Pool = pool;

    public ElasticTest() : this(PersistenceTestBase.JsonSize, new DefaultObjectPool<ElasticsearchPooledObject>(new DefaultPooledObjectPolicy<ElasticsearchPooledObject>()))
    {

    }

    public ElasticTest(bool create) : this(JsonSize,new DefaultObjectPool<ElasticsearchPooledObject>(new DefaultPooledObjectPolicy<ElasticsearchPooledObject>()))
    {
        if (create)
            CreateDB();
    }

    public override void CreateDB()
    {

    }

    public override void CreateIndex()
    {
        // dynamic mapping
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

            var result = lease.Client.IndexAsync(test, (IndexName)"json_data").GetAwaiter().GetResult();
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
            var match = lease.Client.GetAsync<PersistenceTestModel>((IndexName)"json_data", i.ToString()).GetAwaiter().GetResult();
            var cc = match.Source;
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
            var match = await lease.Client.IndexAsync(message, (IndexName)"country_postal");
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
        return await WritePostal(message);
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = Pool.Get();

        try
        {
            var match = await lease.Client.GetAsync<CountryPostalCode>((IndexName)"country_postal", message.Id.ToString());
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
                .Index("country_postal")
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
            var match = await lease.Client.GetAsync<CountryPostalCode>((IndexName)"country_postal", message.Id.ToString());
            var cc = match.Source;

            var response = await lease.Client.SearchAsync<CountryPostalCode>(s => s
                .Index("country_postal")
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