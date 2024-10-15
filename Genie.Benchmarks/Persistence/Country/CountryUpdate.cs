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
    public class CountryUpdate
    {
        public const int SampleSize = 20;
        public const int Count = 250000;

        public static async Task OpTest(PersistenceTestBase test, int threads)
        {
            var logger = new CounterConsoleLogger();

            Console.WriteLine($@"Load Started: {DateTime.Now}");

            var pump = CountryPump.Run(SampleSize, async message =>
            {
                var response = await test.WritePostal(message);

            }, threads);

            await pump.Completion;

            Console.WriteLine($@"Started: {DateTime.Now}");

            var random = new Random();
            
            var timerPump = CountryPump.Run(Count, async message =>
            {
                if (message.Id < SampleSize)
                    return;

                message.PlaceName = $@"Tagged by {message.Id}";
                message.Id = random.Next(1, SampleSize);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var response = await test.UpdatePostal(message);

                stopwatch.Stop();

                if (!response)
                    logger.ProcessError();

                logger.Process(stopwatch.ElapsedMilliseconds);
            }, threads);

            await timerPump.Completion;

            logger.Print();
        }

        public static async Task Aerospike()
        {
            await OpTest(new AerospikeTest(), 32);
        }

        public static async Task Arango()
        {
            await OpTest(new ArangoTest(true), 1);
        }

        public static async Task Cassandra()
        {
            await OpTest(new CassandraTest(true), 2);
        }

        public static async Task ClickHouse()
        {
            await OpTest(new ClickHouseTest(true), 2);
            // 1 - 287
            // 2 - 464
        }

        public static async Task Cockroach()
        {
            await OpTest(new CockroachTest(true), 16);

            // 8 1273
            // 12 1629
            // 16 1444
        }


        public static async Task Couchbase()
        {
            await OpTest(new CouchbaseTest(true), 1);
        }

        public static async Task Couch()
        {
            await OpTest(new CouchTest(true), 16);

            // 1 170
            // 2 260
            // 4 256
            // 6 319
            // 8 374
            // 12 452
            // 16 423
        }

        public static async Task Crate()
        {
            await OpTest(new CrateTest(true), 32);
      
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

        public static async Task DB2()
        {
            await OpTest(new DB2Test(true), 64);
          
            // 32 - 6424
            // 64 - 7970
        }

        public static async Task Elastic()
        {
            await OpTest(new ElasticTest(true), 4);
          
            // 1 - 253
            // 2 - 425
            // 4 - 487
            // 8 - 450
        }

        public static async Task Maria()
        {
            await OpTest(new MariaTest(true), 32);
         
            // 1 - 1045
            // 8 - 3404
            // 16 - 4597
            // 32 - 8298
            // 64 - 8420
        }

        public static async Task Marten()
        {
            await OpTest(new MartenTest(), 32);
        }

        public static async Task Milvus()
        {
            await OpTest(new MilvusTest(true), 32);
        }

        public static async Task Mongo()
        {
            await OpTest(new MongoTest(true), 32);
        }

        public static async Task Mysql()
        {
            var test = new MariaTest();
            test.CreateMySqlDB();

            await OpTest(test, 32);
            // 16 - 2700
            // 32 - 4200
            // 64 - 3196
        }

        public static async Task Neo4j()
        {
            await OpTest(new Neo4jTest(true), 1);
       
            // 1 - 220
            // 2 - 162
        }

        public static async Task Oracle()
        {
            await OpTest(new OracleTest(true), 32);

            // 32a - 774
        }

        public static async Task Postgres()
        {
            await OpTest(new PostgresTest(true), 32);
        }

        public static async Task Raven()
        {
            await OpTest(new RavenTest(true), 32);
            // 32 - 525
            // 32a - 503
            // 64 - 493
        }

        public static async Task Redis()
        {
            await OpTest(new RedisTest(true), 32);
        }

        public static async Task Scylla()
        {
            await OpTest(new ScyllaTest(true), 8);
      
            // 1 = 325
            // 2 = 500
            // 4 = 316
        }

        public static async Task SingleStore()
        {
            await OpTest(new SingleStoreTest(true), 16);
           
            // 1a - 822
            // 8a - 682
            // 8 - 5723
            // 16 - 7064
        }
        public static async Task SqlServer()
        {
            await OpTest(new SqlServerTest(true), 64);
          
            // 32 - 3495
        }
    }
}
