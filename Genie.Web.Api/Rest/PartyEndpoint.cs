using Chr.Avro.Abstract;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Genie.Actors;
using Genie.Adapters.Brokers.ActiveMQ;
using Genie.Adapters.Brokers.Aeron;
using Genie.Adapters.Brokers.Kafka;
using Genie.Adapters.Brokers.MQTT;
using Genie.Adapters.Brokers.NATS;
using Genie.Adapters.Brokers.Pulsar;
using Genie.Adapters.Brokers.RabbitMQ;
using Genie.Adapters.Brokers.ZeroMQ;
using Genie.Adapters.Persistence.Aerospike;
using Genie.Adapters.Persistence.ArangoDB;
using Genie.Adapters.Persistence.Cassandra;
using Genie.Adapters.Persistence.CockroachDB;
using Genie.Adapters.Persistence.Couchbase;
using Genie.Adapters.Persistence.CouchDB;
using Genie.Adapters.Persistence.CrateDB;
using Genie.Adapters.Persistence.Elasticsearch;
using Genie.Adapters.Persistence.MariaDB;
using Genie.Adapters.Persistence.Marten;
using Genie.Adapters.Persistence.Milvus;
using Genie.Adapters.Persistence.MongoDB;
using Genie.Adapters.Persistence.Neo4j;
using Genie.Adapters.Persistence.RavenDB;
using Genie.Adapters.Persistence.Redis;
using Genie.Adapters.Persistence.Scylla;
using Genie.Common.Performance;
using Genie.Common.Utils;
using Genie.Extensions.Genius.Commands;
using Genie.Grpc;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Proto;
using System.Net;

namespace Genie.Web.Api.Rest
{
    public static class PartyEndpoints
    {
        private static readonly byte[] kyber_dilithium = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\kyber_dilithium.req");
        private static readonly byte[] kyber_ed25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\kyber_ed25519.req");
        private static readonly byte[] x25519_dilithium = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\x25519_dilithium.req");
        private static readonly byte[] x25519_ed25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\x25519_ed25519.req");
        private static Random rnd = new Random();
        private static CounterConsoleLogger timer = new();
        private const int payload = 4000;
        private const int maxItemCount = int.MaxValue;

        public static void Map(WebApplication app)
        {
            app.MapGet("test", async () =>
            {
                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("aero", async (ObjectPool<AerospikePooledObject> geniePool) =>
            {
                timer.Process();

                var test = new AerospikeTest(payload, geniePool);

                var rando = rnd.Next(100000);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("arango", async (ObjectPool<ArangoPooledObject> geniePool) =>
            {
                timer.Process();

                var test = new ArangoTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("cassandra", async (ObjectPool<CassandraPooledObject> geniePool) =>
            {
                timer.Process();

                var test = new CassandraTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("cockroach", async (ObjectPool<CockroackPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new CockroachTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("couchbase", async (ObjectPool<CouchbasePooledObject> geniePool) =>
            {
                timer.Process();
                var test = new CouchbaseTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("couch", async (ObjectPool<CouchPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new CouchTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("crate", async (ObjectPool<CratePooledObject> geniePool) =>
            {
                timer.Process();
                var test = new CrateTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("elastic", async (ObjectPool<ElasticsearchPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new ElasticTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("maria", async (ObjectPool<MariaPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new MariaTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("marten", async (ObjectPool<MartenPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new MartenTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });


            app.MapGet("milvus", async (ObjectPool<MilvusPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new MilvusTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });


            app.MapGet("mongo", async (ObjectPool<MongoPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new MongoTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });


            app.MapGet("neo4j", async (ObjectPool<Neo4jPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new Neo4jTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("raven", async (ObjectPool<RavenPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new RavenTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("redis", async (ObjectPool<RedisPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new RedisTest(payload, geniePool);

                var rando = rnd.Next(100000);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("scylla", async (ObjectPool<ScyllaPooledObject> geniePool) =>
            {
                timer.Process();
                var test = new ScyllaTest(payload, geniePool);

                var rando = rnd.Next(maxItemCount);
                test.Write(rando);
                test.Read(rando);

                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapPost("encryption", async
                (ObjectPool<GeniePooledObject> geniePool,
                ActorSystem actorSystem,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var cmd = new NetworkBenchmarkHashedGeniusCommand(GeniusEventRequest.Parser, httpContext, geniePool, actorSystem, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("kd", async
                    (ObjectPool<GeniePooledObject> geniePool,
                    ActorSystem actorSystem,
                    HttpContext httpContext,
                    IMediator mediator) =>
            {
                var cmd = new BenchmarkHashedGeniusCommand(GeniusEventRequest.Parser, kyber_dilithium, httpContext, geniePool, actorSystem, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("ke", async
                (ObjectPool<GeniePooledObject> geniePool,
                ActorSystem actorSystem,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var cmd = new BenchmarkHashedGeniusCommand(GeniusEventRequest.Parser, kyber_ed25519, httpContext, geniePool, actorSystem, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("xd", async
                    (ObjectPool<GeniePooledObject> geniePool,
                    ActorSystem actorSystem,
                    HttpContext httpContext,
                    IMediator mediator) =>
            {
                var cmd = new BenchmarkHashedGeniusCommand(GeniusEventRequest.Parser, x25519_dilithium, httpContext, geniePool, actorSystem, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("xe", async
                    (ObjectPool<GeniePooledObject> geniePool,
                    ActorSystem actorSystem,
                    HttpContext httpContext,
                    IMediator mediator) =>
            {
                var cmd = new BenchmarkHashedGeniusCommand(GeniusEventRequest.Parser, x25519_ed25519, httpContext, geniePool, actorSystem, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("mqtt", async
                (ObjectPool<MQTTPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new MQTTCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("mqtt.fire", async
                (ObjectPool<MQTTPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new MQTTCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("nats", async
                (ObjectPool<NatsPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new NatsCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("nats.fire", async
                (ObjectPool<NatsPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new NatsCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("aeron.service", async
                (ObjectPool<AeronServicePooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new AeronServiceCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("aeron", async
                (ObjectPool<AeronPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new AeronCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("aeron.fire", async
                (ObjectPool<AeronPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new AeronCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("zero", async
                (ObjectPool<ZeroMQPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new ZeroMQCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("zero.fire", async
                (ObjectPool<ZeroMQPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new ZeroMQCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("zlogger", async(IMediator mediator, ILogger<PartyRequest> logger) =>
            {
                var cmd = new ZloggerCommand(logger);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("genius", async
                (ObjectPool<GeniePooledObject> geniePool,
                ActorSystem actorSystem,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var cmd = new GeniusCommand(null, null, null, geniePool, actorSystem, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("proto.actor", async
                (ObjectPool<GeniePooledObject> geniePool,
                ActorSystem actorSystem,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var cmd = new ActorCommand(geniePool, actorSystem, false, httpContext);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("proto.actor.fire", async
                (ObjectPool<GeniePooledObject> geniePool,
                ActorSystem actorSystem,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var cmd = new ActorCommand(geniePool, actorSystem, true, httpContext);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });


            app.MapGet("kafka", async 
                (ObjectPool<KafkaPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                IAdminClient adminClient, 
                IProducer<string, Genie.Common.Types.PartyRequest> producer, 
                CachedSchemaRegistryClient schemaRegistry,  
                IMediator mediator) =>
            {
                var cmd = new KafkaCommand(geniePool, adminClient, producer, schemaRegistry, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("kafka.fire", async
                (ObjectPool<KafkaPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                IAdminClient adminClient,
                IProducer<string, Genie.Common.Types.PartyRequest> producer,
                CachedSchemaRegistryClient schemaRegistry,
                IMediator mediator) =>
            {
                var cmd = new KafkaCommand(geniePool, adminClient, producer, schemaRegistry, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("pulsar", async
                (ObjectPool<PulsarPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                IMediator mediator) =>
            {
                var cmd = new PulsarCommand(geniePool, schemaBuilder, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("pulsar.fire", async
                (ObjectPool<PulsarPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                IMediator mediator) =>
            {
                var cmd = new PulsarCommand(geniePool, schemaBuilder, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("rabbit.fire", async
                (ObjectPool<RabbitMQPooledObject> geniePool,
                SchemaBuilder schemaBuilder, 
                IMediator mediator) => 
            {
                var cmd = new RabbitMQCommand(geniePool, schemaBuilder, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("rabbit", async
                (ObjectPool<RabbitMQPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                IMediator mediator) =>
            {
                var cmd = new RabbitMQCommand(geniePool, schemaBuilder, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("active.fire", async
                (ObjectPool<ActiveMQPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new ActiveMQCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("active", async
                (ObjectPool<ActiveMQPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new ActiveMQCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });
        }
    }
}

