using Chr.Avro.Abstract;
using Genie.Common.Adapters;
using Genie.Common;
using Microsoft.Extensions.ObjectPool;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using Microsoft.Extensions.Logging;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Genie.Adapters.Brokers.ActiveMQ;

public record ActiveMQCommand(ObjectPool<ActiveMQPooledObject> GeniePool, SchemaBuilder SchemaBuilder, ILogger<Exception> Logger, bool FireAndForget) : IRequest<Unit>;

public class ActiveMQCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<ActiveMQCommand, Unit>
{
    public async ValueTask<Unit> Handle(ActiveMQCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var grpc = MockPartyCreator.GetParty();
            var partyRequest = CosmosAdapter.ToCosmos(grpc);
            ActiveMQPooledObject pooledObj = command.GeniePool.Get();

            var bytes = Any.Pack(grpc).ToByteArray();

            if (pooledObj.Counter == 0)
                pooledObj.Configure(this.Context);

            var request = pooledObj.IngressSession?.CreateBytesMessage(bytes)!;
            request.NMSCorrelationID = pooledObj.EventChannel;

            pooledObj!.Producer?.SendAsync(request);

            Apache.NMS.IMessage? result = null;
            if (!command.FireAndForget)
                result = pooledObj.Consumer!.Receive();

            pooledObj.Counter++;
            command.GeniePool.Return(pooledObj);

            if (result != null || command.FireAndForget)
                return await Task.FromResult(new Unit());
            else
                throw new Exception("No Response from server............................................");
        }
        catch(Exception ex)
        {
            command.Logger.LogError(ex, "ActiveMQCommandHandler");
        }

        throw new BadHttpRequestException("Server response was invalid");
    }
}