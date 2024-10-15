
using Genie.Adapters.Persistence.Elasticsearch;
using Microsoft.Extensions.ObjectPool;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public class ElasticBenchmarks : PersistenceBase
{
    public ElasticBenchmarks() {
        persistenceTest = new ElasticTest(payload, new DefaultObjectPool<ElasticsearchPooledObject>(new DefaultPooledObjectPolicy<ElasticsearchPooledObject>()));
    }
}