using Chr.Avro.Abstract;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Genie.Actors;
using Genie.Common.Performance;
using Genie.Extensions.Genius.Commands;
using Genie.Grpc;
using Genie.Web.Api.Common;
using Genie.Web.Api.Mediator.Commands;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Proto;
using System.Net;

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

