using Genie.Common;
using Mediator;
using Genie.Common.Adapters;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using Chr.Avro.Abstract;
using Genie.Web.Api.Common;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using System.Diagnostics;

namespace Genie.Web.Api.Mediator.Commands;

public record RabbitMQCommand(ObjectPool<RabbitMQPooledObject> GeniePool, SchemaBuilder SchemaBuilder, bool FireAndForget) : IRequest;

public class RabbitMQCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<RabbitMQCommand>
{
    public async ValueTask<Unit> Handle(RabbitMQCommand command, CancellationToken cancellationToken)
    {
        var grpc = MockPartyCreator.GetParty();


        var pooledObj = command.GeniePool.Get();

        var bytes = Any.Pack(grpc).ToByteArray();

        if (pooledObj.Counter == 0)
            pooledObj.Configure(command.SchemaBuilder, this.Context);

        var props = pooledObj.Ingress!.CreateBasicProperties();
        props.ReplyTo = command.FireAndForget ? null : pooledObj.EventChannel;
        pooledObj.Ingress.BasicPublish(this.Context.RabbitMQ.Exchange, this.Context.RabbitMQ.RoutingKey, props, bytes);

        var success = command.FireAndForget || pooledObj.ReceiveSignal.WaitOne(30000);

        //if (pooledObj.Counter < 50000)
        //    pooledObj.Counter++;
        //else {
        //    pooledObj.Reset();
        //    pooledObj.Counter = 0;
        //}
        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        if(command.FireAndForget)
            return await Task.FromResult(new Unit());
        else if (pooledObj.Result?.Status == Genie.Common.Types.EventTaskJobStatus.Errored)
            throw new Exception("Actor Error: " + pooledObj.Result?.Exception);
        else if (!success)
            throw new Exception("No Response from server............................................");
        else
            return new Unit();
    }
}