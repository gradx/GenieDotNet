using Genie.Common;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Genie.Web.Api.Common;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;

namespace Genie.Web.Api.Mediator.Commands;

public record PulsarCommand(ObjectPool<PulsarPooledObject> GeniePool, bool FireAndForget) : IRequest;

public class PulsarCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<PulsarCommand>
{
    public async ValueTask<Unit> Handle(PulsarCommand command, CancellationToken cancellationToken)
    {
        var grpc = MockPartyCreator.GetParty();
        var pooledObj = command.GeniePool.Get();

        if (pooledObj.Counter == 0)
            pooledObj.Configure(Context);

        var bytes = Any.Pack(grpc).ToByteArray();


        _ = await pooledObj.Producer!.SendAsync(pooledObj.Producer.NewMessage(bytes, key: command.FireAndForget ? null : pooledObj.EventChannel));

        if (!command.FireAndForget)
        {
            var message = await pooledObj.Consumer!.ReceiveAsync(cancellationToken);
            _ = message.GetValue();

            await pooledObj.Consumer.AcknowledgeAsync(message.MessageId);
        }

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        return new Unit();
    }
}