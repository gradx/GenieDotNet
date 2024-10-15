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
using Genie.Adapters.Persistence.Milvus;
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

namespace Genie.Benchmarks.Persistence.Balance
{
    public class Quad
    {
        public const int Seconds = 300;
        public const int Threads = 32;

        public static async Task OpTest(PersistenceTestBase test)
        {
            var logger = new CounterConsoleLogger();

            Console.WriteLine($@"Started: {DateTime.Now}");

            var pump = CountryTimerPump.Run(Seconds, async message =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var write_response = await test.WritePostal(message);
                bool read_response = false;
                bool query_response = false;
                bool join_response = false;

                if (write_response)
                    read_response = await test.ReadPostal(message);

                if (read_response)
                    query_response = await test.QueryPostal(message);

                if (query_response)
                    join_response = await test.SelfJoinPostal(message);

                stopwatch.Stop();

                if (!write_response || !read_response || !query_response || !join_response)
                    logger.ProcessError();

                logger.Process(stopwatch.ElapsedMilliseconds);
            }, Threads);

            await pump.Completion;

            logger.Print();
        }

        public static async Task Aerospike()
        {
            await OpTest(new AerospikeTest());
        }

        public static async Task Arango()
        {
            await OpTest(new ArangoTest(true));
        }

        public static async Task Cassandra()
        {
            await OpTest(new CassandraTest(true));
        }

        public static async Task ClickHouse()
        {
            await OpTest(new ClickHouseTest(true));
        }

        public static async Task Cockroach()
        {
            await OpTest(new CockroachTest(true));
        }

        public static async Task Couchbase()
        {
            await OpTest(new CouchbaseTest(true));
        }

        public static async Task Couch()
        {
            await OpTest(new CouchTest(true));
        }

        public static async Task Crate()
        {
            await OpTest(new CrateTest(true));
        }

        public static async Task DB2()
        {
            await OpTest(new DB2Test(true));
        }
        public static async Task Elastic()
        {
            await OpTest(new ElasticTest(true));
        }


        public static async Task Maria()
        {
            await OpTest(new MariaTest(true));
        }

        public static async Task Marten()
        {
            await OpTest(new MartenTest());
        }

        public static async Task Milvus()
        {
            await OpTest(new MilvusTest(true));
        }

        public static async Task Mongo()
        {
            await OpTest(new MongoTest(true));
        }

        public static async Task Mysql()
        {
            await OpTest(new MariaTest(true));
        }

        public static async Task Neo4j()
        {
            await OpTest(new Neo4jTest(true));
        }

        public static async Task Oracle()
        {
            var test = new OracleTest(true);
            await OpTest(test);
        }

        public static async Task Postgres()
        {
            await OpTest(new PostgresTest(true));
        }

        public static async Task Raven()
        {
            await OpTest(new RavenTest(true));
        }

        public static async Task Redis()
        {
            await OpTest(new RedisTest(true));
        }

        public static async Task Scylla()
        {
            await OpTest(new ScyllaTest(true));
        }

        public static async Task SingleStore()
        {
            await OpTest(new SingleStoreTest(true));
        }

        public static async Task SqlServer()
        {
            await OpTest(new SqlServerTest(true));
        }
    }
}
