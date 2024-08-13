using Azure.Core;
using Chr.Avro.Abstract;

using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Genie.Actors;
using Genie.Common.Web;
using Genie.Extensions.Commands;
using Genie.Grpc;
using Genie.Web.Api.Common;
using Genie.Web.Api.Mediator.Commands;
using Mediator;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;
using Proto;
using ProtoBuf.Reflection;
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

            //app.MapGet("proto.actor", async 
            //    (ObjectPool<GeniePooledObject> geniePool, 
            //    ActorSystem actorSystem, 
            //    HttpContext httpContext,
            //    IMediator mediator) =>
            //{
            //    var cmd = new ActorCommand(geniePool, actorSystem, false, httpContext);
            //    var result = await mediator.Send(cmd);
            //    return HttpStatusCode.OK;
            //});

            //app.MapGet("proto.actor.fire", async
            //    (ObjectPool<GeniePooledObject> geniePool,
            //    ActorSystem actorSystem,
            //    HttpContext httpContext,
            //    IMediator mediator) =>
            //{
            //    var cmd = new ActorCommand(geniePool, actorSystem, true, httpContext);
            //    var result = await mediator.Send(cmd);
            //    return HttpStatusCode.OK;
            //});


            app.MapGet("kafka", async 
                (ObjectPool<KafkaPooledObject> geniePool, 
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
                IMediator mediator) =>
            {
                var cmd = new PulsarCommand(geniePool, false);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });

            app.MapGet("pulsar.fire", async
                (ObjectPool<PulsarPooledObject> geniePool,
                IMediator mediator) =>
            {
                var cmd = new PulsarCommand(geniePool, true);
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

            app.MapGet("active", async
                (ObjectPool<ActiveMQPooledObject> geniePool,
                SchemaBuilder schemaBuilder,
                ILogger<Exception> logger,
                IMediator mediator) =>
            {
                var cmd = new ActiveMQCommand(geniePool, schemaBuilder, logger);
                var result = await mediator.Send(cmd);
                return HttpStatusCode.OK;
            });
        }
    }
}

