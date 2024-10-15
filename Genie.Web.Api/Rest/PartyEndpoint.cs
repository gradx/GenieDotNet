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
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Core;
using Genie.Extensions.Commands;
using Genie.Web.Api.Commands;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Proto;
using System.Net;
using static Genie.Common.Adapters.CosmosAdapter;

namespace Genie.Web.Api.Rest
{
    public static class PartyEndpoints
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("test", async () =>
            {
                return await Task.FromResult(HttpStatusCode.OK);
            });

            app.MapGet("mqtt", async
                (ObjectPool<MQTTPooledObject<Common.Types.EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new MQTTCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("mqtt.fire", async
                (ObjectPool<MQTTPooledObject<EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new MQTTCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("nats", async
                (ObjectPool<NatsPooledObject<EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new NatsCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("nats.fire", async
                (ObjectPool<NatsPooledObject<EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new NatsCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("aeron.service", async
                (ObjectPool<AeronServicePooledObject<EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new AeronServiceCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("aeron", async
                (ObjectPool<AeronPooledObject<Common.Types.EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new AeronCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("aeron.fire", async
                (ObjectPool<AeronPooledObject<EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new AeronCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("zero", async
                (ObjectPool<ZeroMQPooledObject<EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new ZeroMQCommand(geniePool, schemaBuilder, logger, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("zero.fire", async
                (ObjectPool<ZeroMQPooledObject<EventTaskJob>> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new ZeroMQCommand(geniePool, schemaBuilder, logger, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("zlogger", async(IMediator mediator, ILogger<Grpc.PartyRequest> logger) =>
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
                (ObjectPool<KafkaPooledObject<EventTaskJob>> geniePool,
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
                (ObjectPool<KafkaPooledObject<EventTaskJob>> geniePool,
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
                (ObjectPool<RabbitMQPooledObject<EventTaskJob, PartyBenchmarkRequest>> geniePool,
                SchemaBuilder schemaBuilder, 
                IMediator mediator) => 
            {
                var cmd = new RabbitMQCommand(geniePool, schemaBuilder, true);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("rabbit", async
                (ObjectPool<RabbitMQPooledObject<EventTaskJob, PartyBenchmarkRequest>> geniePool,
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

