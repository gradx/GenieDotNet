using Chr.Avro.Abstract;
using Genie.Common.Adapters;
using Genie.Common;
using Genie.Web.Api.Common;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using Genie.Common.Types;
namespace Genie.Web.Api.Mediator.Commands;

public record MQTTCommand(ObjectPool<MQTTPooledObject> GeniePool, SchemaBuilder SchemaBuilder, ILogger<Exception> Logger, bool FireAndForget) : IRequest<Unit>;

public class MQTTCommandCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<MQTTCommand, Unit>
{
    public async ValueTask<Unit> Handle(MQTTCommand command, CancellationToken cancellationToken)
    {
        try
        {

            MQTTPooledObject pooledObj = command.GeniePool.Get();
            var grpc = MockPartyCreator.GetParty();
            grpc.Request.Info = pooledObj.EventChannel;

            var partyRequest = CosmosAdapter.ToCosmos(grpc);

            var bytes = Any.Pack(grpc).ToByteArray();

            if (pooledObj.Counter == 0)
                pooledObj.Configure(command.SchemaBuilder, this.Context);

            pooledObj.Send(bytes);

            var success = command.FireAndForget || pooledObj.ReceiveSignal.WaitOne(30000);

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
        catch(Exception ex)
        {
            command.Logger.LogError(ex, "ActiveMQCommandHandler");
        }

        throw new BadHttpRequestException("Server response was invalid");
    }
}