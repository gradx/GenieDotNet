
using Genie.Adapters.Persistence.Cassandra;
using Microsoft.Extensions.ObjectPool;


namespace Genie.Benchmarks.Benchmarks.Persistence;

public class CassandraBenchmarks : PersistenceBase
{
    public CassandraBenchmarks() {
        persistenceTest = new CassandraTest(payload, new DefaultObjectPool<CassandraPooledObject>(new DefaultPooledObjectPolicy<CassandraPooledObject>()));
    }
}