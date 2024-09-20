
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Adapters.Persistence.Elasticsearch;

public class ElasticTest(int payload, ObjectPool<ElasticsearchPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<ElasticsearchPooledObject> Pool = pool;
    

    public static void CreateDB()
    {

    }

    public void Write(int i)
    {
        var test = new PersistenceTest
        {
            Id = $@"new{i}",
            Info = new('-', Payload)
        };


        var lease = Pool.Get();

        var result = lease.Client.IndexAsync(test).GetAwaiter().GetResult();
        //var result = lease.Client.UpdateAsync<PersistenceTest, PersistenceTest>("genie", $@"new{i}", i => i.Doc(test)).GetAwaiter().GetResult();

        Pool.Return(lease);
    }

    public void Read(int i)
    {

        var lease = Pool.Get();

        var result = lease.Client.GetAsync<PersistenceTest>("genie", $@"new{i}").GetAwaiter().GetResult();

        Pool.Return(lease);
    }
}