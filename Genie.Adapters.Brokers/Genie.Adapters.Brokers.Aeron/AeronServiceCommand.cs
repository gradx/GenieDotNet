using Chr.Avro.Abstract;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using Genie.Common.Types;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Genie.Common;

namespace Genie.Adapters.Brokers.Aeron;

public record AeronServiceCommand(ObjectPool<AeronServicePooledObject> GeniePool, SchemaBuilder SchemaBuilder, ILogger<Exception> Logger, bool FireAndForget) : IRequest<Unit>;

public class AeronServiceCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<AeronServiceCommand, Unit>
{
    public async ValueTask<Unit> Handle(AeronServiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            AeronServicePooledObject pooledObj = command.GeniePool.Get();


            if (pooledObj.Counter == 0)
                pooledObj.Configure(command.SchemaBuilder, this.Context);

            var grpc = MockPartyCreator.GetParty();

            grpc.Party.CosmosBase.Identifier.Id = pooledObj.Counter + "_" + pooledObj.EventChannel + "_" + grpc.Party.CosmosBase.Identifier.Id;

            //grpc.Request.Info = pooledObj.Subscription.Subscription.Channel;

            var bytes = Any.Pack(grpc).ToByteArray();


            pooledObj.Send(bytes);

            var success = command.FireAndForget || pooledObj.ReceiveSignal.WaitOne(30000);

            EventTaskJob? result = null;


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
        catch (Exception ex)
        {
            command.Logger.LogError(ex, "ActiveMQCommandHandler");
        }

        throw new BadHttpRequestException("Server response was invalid");
    }
}