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

namespace Genie.Benchmarks.Persistence.Country
{
    public class CountryWrite
    {
        public static async Task OpTest(PersistenceTestBase test, int threads)
        {
            var logger = new CounterConsoleLogger();

            Console.WriteLine($@"Started: {DateTime.Now}");

            var pump = CountryPump.Run(async message =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var response = await test.WritePostal(message);

                stopwatch.Stop();

                if (!response)
                    logger.ProcessError();

                logger.Process(stopwatch.ElapsedMilliseconds);
            }, threads);

            await pump.Completion;

            logger.Print();
        }

        public static async Task Aerospike(int threads = 32)
        {
            await OpTest(new AerospikeTest(), threads);
        }

        public static async Task Arango(int threads = 1)
        {
            await OpTest(new ArangoTest(true), threads);
        }

        public static async Task Cassandra(int threads = 2)
        {
            await OpTest(new CassandraTest(true), threads);
        }

        public static async Task ClickHouse(int threads = 2)
        {
            await OpTest(new ClickHouseTest(true), threads);
            // 1 - 287
            // 2 - 464
        }

        public static async Task Cockroach(int threads = 16)
        {
            await OpTest(new CockroachTest(true), threads);

            // 8 1273
            // 12 1629
            // 16 1444
        }


        public static async Task Couchbase(int threads = 1)
        {
            await OpTest(new CouchbaseTest(true), threads);
        }

        public static async Task Couch(int threads = 16)
        {
            await OpTest(new CouchTest(true), threads);

            // 1 170
            // 2 260
            // 4 256
            // 6 319
            // 8 374
            // 12 452
            // 16 423
        }

        public static async Task Crate(int threads = 32)
        {
            await OpTest(new CrateTest(true), threads);
      
            // 1a 317
            // 2a 482
            // 4a 312
            // 1 406 
            // 2 634
            // 4 909
            // 8 1371
            // 16 2141
            // 32 3613
            // 64 2632
        }

        public static async Task DB2(int threads = 64)
        {
            await OpTest(new DB2Test(true), threads);
          
            // 32 - 6424
            // 64 - 7970
        }

        public static async Task Elastic(int threads = 4)
        {
            await OpTest(new ElasticTest(true), threads);
          
            // 1 - 253
            // 2 - 425
            // 4 - 487
            // 8 - 450
        }

        public static async Task Maria(int threads = 32)
        {
            await OpTest(new MariaTest(true), threads);
         
            // 1 - 1045
            // 8 - 3404
            // 16 - 4597
            // 32 - 8298
            // 64 - 8420
        }

        public static async Task Marten(int threads = 32)
        {
            await OpTest(new MartenTest(), threads);
        }

        public static async Task Milvus(int threads = 16)
        {
            await OpTest(new MilvusTest(true), threads);
        }

        public static async Task Mongo(int threads = 32)
        {
            await OpTest(new MongoTest(true), threads);
        }

        public static async Task Mysql(int threads = 32)
        {
            var test = new MariaTest();
            test.CreateMySqlDB();

            await OpTest(test, threads);
            // 16 - 2700
            // 32 - 4200
            // 64 - 3196
        }

        public static async Task Neo4j(int threads = 1)
        {
            await OpTest(new Neo4jTest(true), threads);
       
            // 1 - 220
            // 2 - 162
        }

        public static async Task Oracle(int threads = 32)
        {
            await OpTest(new OracleTest(true), threads);

            // 32a - 774
        }

        public static async Task Postgres(int threads = 32)
        {
            await OpTest(new PostgresTest(true), threads);
        }

        public static async Task Raven(int threads = 32)
        {
            await OpTest(new RavenTest(true), threads);
            // 32 - 525
            // 32a - 503
            // 64 - 493
        }

        public static async Task Redis(int threads = 32)
        {
            await OpTest(new RedisTest(true), threads);
        }

        public static async Task Scylla(int threads = 8)
        {
            await OpTest(new ScyllaTest(true), threads);
      
            // 1 = 325
            // 2 = 500
            // 4 = 316
        }

        public static async Task SingleStore(int threads = 16)
        {
            await OpTest(new SingleStoreTest(true), threads);
           
            // 1a - 822
            // 8a - 682
            // 8 - 5723
            // 16 - 7064
        }
        public static async Task SqlServer(int threads = 64)
        {
            await OpTest(new SqlServerTest(true), threads);
          
            // 32 - 3495
        }
    }
}
