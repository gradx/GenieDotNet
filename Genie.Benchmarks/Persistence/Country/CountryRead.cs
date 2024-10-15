using Genie.Adapters.Persistence.Aerospike;
using Genie.Adapters.Persistence.ArangoDB;
using Genie.Adapters.Persistence.Cassandra;
using Genie.Adapters.Persistence.ClickHouse;
using Genie.Adapters.Persistence.CockroachDB;
using Genie.Adapters.Persistence.Couchbase;
using Genie.Adapters.Persistence.Couch;
using Genie.Adapters.Persistence.CrateDB;
using Genie.Adapters.Persistence.DB2;
using Genie.Adapters.Persistence.Elasticsearch;
using Genie.Adapters.Persistence.MariaDB;
using Genie.Adapters.Persistence.Marten;
using Genie.Adapters.Persistence.MongoDB;
using Genie.Adapters.Persistence.Neo4j;
using Genie.Adapters.Persistence.Oracle;
using Genie.Adapters.Persistence.Postgres;
using Genie.Adapters.Persistence.RavenDB;
using Genie.Adapters.Persistence.Redis;
using Genie.Adapters.Persistence.Scylla;
using Genie.Adapters.Persistence.SingleStore;
using Genie.Adapters.Persistence.SqlServer;
using Genie.Common.Utils;
using Genie.Core.Persistence;
using Genie.Core.Pumps;
using System.Diagnostics;
using Genie.Adapters.Persistence.Milvus;

namespace Genie.Benchmarks.Persistence.Country
{
    public class CountryRead
    {
        public const int ReadCount = 250000;

        public static async Task OpTest(PersistenceTestBase test, int threads)
        {
            var logger = new CounterConsoleLogger();

            Console.WriteLine($@"Started: {DateTime.Now}");

            var pump = CountryPump.Run(ReadCount, async message =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var response = await test.ReadPostal(message);

                stopwatch.Stop();

                if (!response)
                    logger.ProcessError();

                logger.Process(stopwatch.ElapsedMilliseconds);
            }, threads);

            await pump.Completion;

            logger.Print();
        }

        public static async Task Aerospike()
        {
            await OpTest(new AerospikeTest(), 32);
        }

        public static async Task Arango()
        {
            await OpTest(new ArangoTest(), 1);
        }

        public static async Task Cassandra()
        {
            await OpTest(new CassandraTest(), 2);
        }

        public static async Task ClickHouse()
        {
            await OpTest(new ClickHouseTest(), 4);
            // 8 - 483
        }

        public static async Task Cockroach()
        {
            await OpTest(new CockroachTest(), 64);
            // 1 1021
            // 4 2500
            // 8 3550
            // 16 3287
            // 32 3600
            // 64 4155
        }

        public static async Task Couchbase()
        {
            await OpTest(new CouchbaseTest(), 1);
        }

        public static async Task Couch()
        {
            await OpTest(new CouchTest(), 2);
        }

        public static async Task Crate()
        {
            await OpTest(new CrateTest(), 32);
            // 1 - 1478
            // 2 - 2040
            // 4 - 2973
            // 8 - 4374
            // 16 - 6948
            // 32 - 10113
        }

        public static async Task DB2()
        {
            await OpTest(new DB2Test(), 32);
            // 32a - 6927
            // 16a - 4795
            // 32 - 5103
        }

        public static async Task Elastic()
        {
            await OpTest(new ElasticTest(), 1);
        }


        public static async Task Maria()
        {
            await OpTest(new MariaTest(), 32);
        }

        public static async Task Marten()
        {
            await OpTest(new MartenTest(), 32);
        }

        public static async Task Milvus()
        {
            await OpTest(new MilvusTest(), 32);
        }


        public static async Task Mongo()
        {
            await OpTest(new MongoTest(), 32);
        }

        public static async Task Mysql()
        {
            await OpTest(new MariaTest(), 32);
        }

        public static async Task Neo4j()
        {
            await OpTest(new Neo4jTest(), 16);
        }

        public static async Task Oracle()
        {
            await OpTest(new OracleTest(), 32);
        }

        public static async Task Postgres()
        {
            await OpTest(new PostgresTest(), 32);
        }

        public static async Task Raven()
        {
            await OpTest(new RavenTest(), 64);
            // 32 - 416
            // 64 - 510
        }

        public static async Task Redis()
        {
            await OpTest(new RedisTest(), 32);
        }

        public static async Task Scylla()
        {
            await OpTest(new ScyllaTest(), 2);
            // 1 = 333
            // 2 = 541
            // 4 = 265
        }

        public static async Task SingleStore()
        {
            await OpTest(new SingleStoreTest(), 16);
        }

        public static async Task SqlServer()
        {
            await OpTest(new SingleStoreTest(), 64);
            // 64 - 
            // 64a - 3529
        }
    }
}
