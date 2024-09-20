using Mediator;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using Chr.Avro.Abstract;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using Genie.Common;

namespace Genie.Adapters.Brokers.RabbitMQ;

public record RabbitMQCommand(ObjectPool<RabbitMQPooledObject> GeniePool, SchemaBuilder SchemaBuilder, bool FireAndForget) : IRequest;

public class RabbitMQCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<RabbitMQCommand>
{
    public async ValueTask<Unit> Handle(RabbitMQCommand command, CancellationToken cancellationToken)
    {

        var grpc = MockPartyCreator.GetParty();

        var pooledObj = command.GeniePool.Get();

        if (pooledObj.Counter == 0)
            await pooledObj.Configure(command.SchemaBuilder, this.Context, cancellationToken);

        var bytes = Any.Pack(grpc).ToByteArray();


        var props = new BasicProperties { ReplyTo = command.FireAndForget ? null : pooledObj.EventChannel };
        await pooledObj.Ingress!.BasicPublishAsync(this.Context.RabbitMQ.Exchange, this.Context.RabbitMQ.RoutingKey, false, props, bytes, cancellationToken);


        var success = command.FireAndForget || pooledObj.ReceiveSignal.WaitOne(30000);

        //if (pooledObj.Counter < 50000)
        //    pooledObj.Counter++;
        //else {
        //    pooledObj.Reset();
        //    pooledObj.Counter = 0;
        //}
        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);


        if (command.FireAndForget)
            return await Task.FromResult(new Unit());
        else if (pooledObj.Result?.Status == Genie.Common.Types.EventTaskJobStatus.Errored)
            throw new Exception("Actor Error: " + pooledObj.Result?.Exception);
        else if (!success)
            throw new Exception("No Response from server............................................");
        else
            return new Unit();
    }
}