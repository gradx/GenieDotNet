
using BenchmarkDotNet.Running;
using Genie.Adapters.Persistence.Aerospike;
using Genie.Adapters.Persistence.ArangoDB;
using Genie.Adapters.Persistence.Cassandra;
using Genie.Adapters.Persistence.CockroachDB;
using Genie.Adapters.Persistence.Couchbase;
using Genie.Adapters.Persistence.CouchDB;
using Genie.Adapters.Persistence.CrateDB;
using Genie.Adapters.Persistence.MariaDB;
using Genie.Adapters.Persistence.Milvus;
using Genie.Adapters.Persistence.MongoDB;
using Genie.Adapters.Persistence.RavenDB;
using Genie.Adapters.Persistence.Redis;
using Genie.Adapters.Persistence.Scylla;
using Genie.Benchmarks.Benchmarks.Persistence;
using Humanizer;
using Microsoft.Extensions.ObjectPool;


//var raven = new RavenTest(4000, new DefaultObjectPool<RavenPooledObject>(new DefaultPooledObjectPolicy<RavenPooledObject>()));
////raven.CreateDB();
//raven.Write(1);
////raven.WriteSelfReference();
//raven.Read(1);
//var a = 0;

//var mongoTest = new MongoTest(4000, new DefaultObjectPool<MongoPooledObject>(new DefaultPooledObjectPolicy<MongoPooledObject>()));
//mongoTest.Write(1);
//mongoTest.Read(1);


//var cassandraTest = new CassandraTest(4000, new DefaultObjectPool<CassandraPooledObject>(new DefaultPooledObjectPolicy<CassandraPooledObject>()));
//cassandraTest.Write(1);
//cassandraTest.Read(1);

//var couch = new CouchTest(4000, new DefaultObjectPool<CouchPooledObject>(new DefaultPooledObjectPolicy<CouchPooledObject>()));
//couch.Write(2);
//couch.Read(2);


//var scylla = new ScyllaTest();
//scylla.Write(1);
//scylla.Read(1);

//var crate = new CrateTest(4000, new DefaultObjectPool<CratePooledObject>(new DefaultPooledObjectPolicy<CratePooledObject>()));
//crate.CreateDB();
//crate.Write(1);
//crate.Read(1);

//var cock = new CockroachTest(4000, new DefaultObjectPool<CockroackPooledObject>(new DefaultPooledObjectPolicy<CockroackPooledObject>()));
//cock.Write(1);
//cock.Read(1);

//await ArangoTest.CreateDB();
//var arango = new ArangoTest(4000, new DefaultObjectPool<ArangoPooledObject>(new DefaultPooledObjectPolicy<ArangoPooledObject>()));
//arango.Write(1);
//arango.Read(1);

//var aero = new AerospikeTest(4000, new DefaultObjectPool<AerospikePooledObject>(new DefaultPooledObjectPolicy<AerospikePooledObject>()));
//aero.Write(1);
//aero.Read(1);

//var scylla = new ScyllaTest(400, new DefaultObjectPool<ScyllaPooledObject>(new DefaultPooledObjectPolicy<ScyllaPooledObject>()));
//scylla.Write(1);
//scylla.Read(1);

//var marten = new MartenTest();
//marten.Write(1);
//marten.Read(1);

//var neo = new Neo4jTest();
//neo.Write(1);
//neo.Read(1);

//var elastic = new ElasticTest();
//elastic.Write(1);
//elastic.Read(1);

//var redis = new RedisTest(4000, new DefaultObjectPool<RedisPooledObject>(new DefaultPooledObjectPolicy<RedisPooledObject>()));
//redis.Write(1);
//redis.Read(1);

var maria = new MariaTest(4000, new DefaultObjectPool<MariaPooledObject>(new DefaultPooledObjectPolicy<MariaPooledObject>()));
//maria.CreateDB();
maria.Write(1);
maria.Read(1);

//var milvus = new MilvusTest(4000, new DefaultObjectPool<MilvusPooledObject>(new DefaultPooledObjectPolicy<MilvusPooledObject>()));
//milvus.Write(1);
//milvus.Read(1);

//var couchbase = new CouchbaseTest(4000, new DefaultObjectPool<CouchbasePooledObject>(new DefaultPooledObjectPolicy<CouchbasePooledObject>()));
//couchbase.Write(1);
//couchbase.Read(1);

//var redis = BenchmarkRunner.Run<RedisBenchmarks>();
//var elastic2 = BenchmarkRunner.Run<ElasticBenchmarks>();
//var neo4j = BenchmarkRunner.Run<Neo4jBenchmarks>();
//var martenBench = BenchmarkRunner.Run<MartenBenchmarks>();
//var arangoBench = BenchmarkRunner.Run<ArangoBenchmarks>();
//var cockBench = BenchmarkRunner.Run<CockroackBenchmarks>();
//var crateBench = BenchmarkRunner.Run<CrateBenchmarks>();
//var scyllaBench = BenchmarkRunner.Run<ScyllaBenchmarks>();
//var couchBench = BenchmarkRunner.Run<CouchBenchmarks>();
//var cassBench = BenchmarkRunner.Run<CassandraBenchmarks>();
//var ravenBench = BenchmarkRunner.Run<RavenBenchmarks>();
//var couchbaseBench = BenchmarkRunner.Run<CouchbaseBenchmarks>();
//var aeroBench = BenchmarkRunner.Run<AerospikeBenchmarks>();
var mongoBench = BenchmarkRunner.Run<MongoBenchmarks>();

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
