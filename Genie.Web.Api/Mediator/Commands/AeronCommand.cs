using Chr.Avro.Abstract;
using Genie.Common;
using Genie.Web.Api.Common;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using Genie.Common.Types;

namespace Genie.Web.Api.Mediator.Commands;

public record AeronCommand(ObjectPool<AeronPooledObject> GeniePool, SchemaBuilder SchemaBuilder, ILogger<Exception> Logger, bool FireAndForget) : IRequest<Unit>;

public class AeronCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<AeronCommand, Unit>
{
    public async ValueTask<Unit> Handle(AeronCommand command, CancellationToken cancellationToken)
    {
        try
        {
            AeronPooledObject pooledObj = command.GeniePool.Get();

            if (pooledObj.Counter == 0)
                pooledObj.Configure(command.SchemaBuilder, this.Context);

            var grpc = MockPartyCreator.GetParty();

            grpc.Party.CosmosBase.Identifier.Id = pooledObj.Counter + "_" + pooledObj.EventChannel + "_" + grpc.Party.CosmosBase.Identifier.Id;

            if (!command.FireAndForget)
                grpc.Request.Info = pooledObj.Subscription.Subscription.Channel;

            var bytes = Any.Pack(grpc).ToByteArray();
            pooledObj.Send(bytes);

            var success = command.FireAndForget || pooledObj.Subscription.ReceiveSignal.WaitOne(30000);

            EventTaskJob? result = null;

            if (!command.FireAndForget)
                result = pooledObj.Deserialize(pooledObj.Subscription.Data);

            pooledObj.Counter++;
            command.GeniePool.Return(pooledObj);

            if (command.FireAndForget)
                return await Task.FromResult(new Unit());
            else if (result?.Status == EventTaskJobStatus.Errored)
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