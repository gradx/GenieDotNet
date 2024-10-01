
using BenchmarkDotNet.Running;
using Genie.Adapters.Persistence.Aerospike;
using Genie.Adapters.Persistence.ArangoDB;
using Genie.Adapters.Persistence.Cassandra;
using Genie.Adapters.Persistence.ClickHouse;
using Genie.Adapters.Persistence.CockroachDB;
using Genie.Adapters.Persistence.Couchbase;
using Genie.Adapters.Persistence.CouchDB;
using Genie.Adapters.Persistence.CrateDB;
using Genie.Adapters.Persistence.DB2;
using Genie.Adapters.Persistence.Elasticsearch;
using Genie.Adapters.Persistence.MariaDB;
using Genie.Adapters.Persistence.Marten;
using Genie.Adapters.Persistence.Milvus;
using Genie.Adapters.Persistence.MongoDB;
using Genie.Adapters.Persistence.Oracle;
using Genie.Adapters.Persistence.Postgres;
using Genie.Adapters.Persistence.RavenDB;
using Genie.Adapters.Persistence.Redis;
using Genie.Adapters.Persistence.Scylla;
using Genie.Adapters.Persistence.SingleStore;
using Genie.Adapters.Persistence.SqlServer;
using Genie.Benchmarks.Benchmarks.Persistence;
using Genie.Utils;
using Humanizer;
using Microsoft.Extensions.ObjectPool;
using Org.BouncyCastle.Ocsp;

//var sql = new SqlServerTest(4000, new DefaultObjectPool<SqlServerPooledObject>(new DefaultPooledObjectPolicy<SqlServerPooledObject>()));
//sql.CreateDB();
//sql.Write(1);
//sql.Read(1);
//sql.Query(1);
//sql.SelfJoin(1);

//var single1 = new SingleStoreTest(4000, new DefaultObjectPool<SingleStorePooledObject>(new DefaultPooledObjectPolicy<SingleStorePooledObject>()));
//single1.CreateDB();
//single1.Write(1);
//single1.Read(1);
//single1.Query(1);
//single1.SelfJoin(1);

//var oracle = new OracleTest(4000, new DefaultObjectPool<OraclePooledObject>(new DefaultPooledObjectPolicy<OraclePooledObject>()));
//oracle.CreateDB();
//oracle.Write(1);
//oracle.Read(1);
//oracle.Query(1);
//oracle.SelfJoin(1);

//var db2 = new DB2Test(4000, new DefaultObjectPool<DB2PooledObject>(new DefaultPooledObjectPolicy<DB2PooledObject>()));
//db2.CreateDB();
//db2.Write(1);
//db2.Read(1);
//db2.Query(1);
//db2.SelfJoin(1);

//var raven = new RavenTest(4000, new DefaultObjectPool<RavenPooledObject>(new DefaultPooledObjectPolicy<RavenPooledObject>()));
//raven.CreateDB();
//raven.Write(1);
//raven.Read(1);
//raven.Query(1);
//raven.SelfJoin(1);

//var mongoTest = new MongoTest(4000, new DefaultObjectPool<MongoPooledObject<PersistenceTest>>(new DefaultPooledObjectPolicy<MongoPooledObject<PersistenceTest>>()));
//mongoTest.Write(1);
//mongoTest.Read(1);
//mongoTest.Query(1);
//mongoTest.SelfJoin(1);

//var clickTest = new ClickHouseTest(4000, new DefaultObjectPool<ClickHousePooledObject>(new DefaultPooledObjectPolicy<ClickHousePooledObject>()));
//clickTest.CreateDB();
//clickTest.Write(1);
//clickTest.Read(1);
//clickTest.Query(1);
//clickTest.SelfJoin(1);

//var cassandraTest = new CassandraTest(4000, new DefaultObjectPool<CassandraPooledObject>(new DefaultPooledObjectPolicy<CassandraPooledObject>()));
//cassandraTest.Write(1);
//cassandraTest.Read(1);
//cassandraTest.Query(1);
//cassandraTest.SelfJoin(1);

//var couch = new CouchTest(4000, new DefaultObjectPool<CouchPooledObject>(new DefaultPooledObjectPolicy<CouchPooledObject>()));
//couch.Write(2);
//couch.Read(2);
//couch.Query(1);
//couch.SelfJoin(1);

//var scylla = new ScyllaTest(4000, new DefaultObjectPool<ScyllaPooledObject>(new DefaultPooledObjectPolicy<ScyllaPooledObject>()));
//scylla.Write(1);
//scylla.Read(1);
//scylla.Query(1);
//scylla.SelfJoin(1);

//var crate = new CrateTest(4000, new DefaultObjectPool<CratePooledObject>(new DefaultPooledObjectPolicy<CratePooledObject>()));
//crate.CreateDB();
//crate.Write(1);
//crate.Read(1);
//crate.Query(1);
//crate.SelfJoin(1);

//var cock = new CockroachTest(4000, new DefaultObjectPool<CockroachPooledObject>(new DefaultPooledObjectPolicy<CockroachPooledObject>()));
//cock.CreateDB();
//cock.Write(1);
//cock.Read(1);
//cock.Query(1);
//cock.SelfJoin(1);

//await ArangoTest.CreateDB();
//var arango = new ArangoTest(4000, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()));
//arango.Write(1);
//arango.Read(1);
//arango.Query(1);
//arango.SelfJoin(1);

//var aero = new AerospikeTest(4000, new DefaultObjectPool<AerospikePooledObject>(new DefaultPooledObjectPolicy<AerospikePooledObject>()));
//aero.Write(1);
//aero.Read(1);

//var scylla = new ScyllaTest(400, new DefaultObjectPool<ScyllaPooledObject>(new DefaultPooledObjectPolicy<ScyllaPooledObject>()));
//scylla.Write(1);
//scylla.Read(1);

//var marten = new MartenTest(4000, new DefaultObjectPool<MartenPooledObject>(new DefaultPooledObjectPolicy<MartenPooledObject>()));
//marten.Write(1);
//marten.Read(1);
//marten.Query(1);
//marten.SelfJoin(1);

//var neo = new Neo4jTest();
//neo.Write(1);
//neo.Read(1);

//var elastic = new ElasticTest(4000, new DefaultObjectPool<ElasticsearchPooledObject>(new DefaultPooledObjectPolicy<ElasticsearchPooledObject>()));
//elastic.Write(1);
//elastic.Read(1);
//elastic.Query(1);
//elastic.SelfJoin(1);

//var redis = new RedisTest(4000, new DefaultObjectPool<RedisPooledObject>(new DefaultPooledObjectPolicy<RedisPooledObject>()));
//redis.Write(1);
//redis.Read(1);

//var maria = new MariaTest(4000, new DefaultObjectPool<MariaPooledObject>(new DefaultPooledObjectPolicy<MariaPooledObject>()));
//maria.CreateDB();
////maria.CreateMySqlDB();
//maria.Write(1);
//maria.Read(1);
//maria.Query(1);
//maria.SelfJoin(1);

//var milvus = new MilvusTest(4000, new DefaultObjectPool<MilvusPooledObject>(new DefaultPooledObjectPolicy<MilvusPooledObject>()));
//milvus.Write(1);
//milvus.Read(1);

//var post = new PostgresTest(4000, new DefaultObjectPool<PostgresPooledObject>(new DefaultPooledObjectPolicy<PostgresPooledObject>()));
//post.CreateDB();
//post.Write(1);
//post.Read(1);
//post.Query(1);
//post.SelfJoin(1);


//var couchbase = new CouchbaseTest(4000, new DefaultObjectPool<CouchbasePooledObject>(new DefaultPooledObjectPolicy<CouchbasePooledObject>()));
//couchbase.Query(1);
//couchbase.SelfJoin(1);
//couchbase.Write(1);
//couchbase.Read(1);


//var aeroBench = BenchmarkRunner.Run<AerospikeBenchmarks>();
//var arangoBench = BenchmarkRunner.Run<ArangoBenchmarks>();
//var cassBench = BenchmarkRunner.Run<CassandraBenchmarks>();
//var clickBench = BenchmarkRunner.Run<ClickHouseBenchmarks>();
//var cockBench = BenchmarkRunner.Run<CockroachBenchmarks>();
//var couchbaseBench = BenchmarkRunner.Run<CouchbaseBenchmarks>();
//var couchBench = BenchmarkRunner.Run<CouchBenchmarks>();
//var crateBench = BenchmarkRunner.Run<CrateBenchmarks>();
//var db2bench = BenchmarkRunner.Run<DB2Benchmarks>();
//var elastic2 = BenchmarkRunner.Run<ElasticBenchmarks>();
//var mariaBench = BenchmarkRunner.Run<MariaBenchmarks>();
//var martenBench = BenchmarkRunner.Run<MartenBenchmarks>();
// milvus
//var mongoBench = BenchmarkRunner.Run<MongoBenchmarks>();
//var neo4j = BenchmarkRunner.Run<Neo4jBenchmarks>();
//var oracleBench = BenchmarkRunner.Run<OracleBenchmarks>();
//var postBench = BenchmarkRunner.Run<PostgresBenchmarks>();
//var ravenBench = BenchmarkRunner.Run<RavenBenchmarks>();
//var redis = BenchmarkRunner.Run<RedisBenchmarks>();
//var scyllaBench = BenchmarkRunner.Run<ScyllaBenchmarks>();
var single = BenchmarkRunner.Run<SingleStoreBenchmarks>();
//var sqlBench = BenchmarkRunner.Run<SqlServerBenchmarks>();



//var symm = BenchmarkRunner.Run<SymmetricalBenchmarks>();

//var results1 = BenchmarkRunner.Run<SecpBenchmarks>();
//var results2 = BenchmarkRunner.Run<CurveBenchmarks>();
//var results3 = BenchmarkRunner.Run<ModuleLatticeBenchmarks>();
//var results4 = BenchmarkRunner.Run<RsaBenchmarks>();
//var results5 = BenchmarkRunner.Run<NistBenchmarks>();

//var results2 = BenchmarkRunner.Run<PqcNetworkBenchmarks>();

//Test256BouncyIntegration();
//Test384BouncyIntegration();
//Test256BouncyIntegration();
//var test = new ModuleLatticeBenchmarks();

//TestBouncy();


//TestBouncy();
