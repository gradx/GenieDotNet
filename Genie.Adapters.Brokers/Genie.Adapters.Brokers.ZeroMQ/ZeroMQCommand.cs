using Chr.Avro.Abstract;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Genie.Common;

namespace Genie.Adapters.Brokers.ZeroMQ;

public record ZeroMQCommand(ObjectPool<ZeroMQPooledObject> GeniePool, SchemaBuilder SchemaBuilder, ILogger<Exception> Logger, bool FireAndForget) : IRequest<Unit>;

public class ZeroMQCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<ZeroMQCommand, Unit>
{
    public async ValueTask<Unit> Handle(ZeroMQCommand command, CancellationToken cancellationToken)
    {
        try
        {
            ZeroMQPooledObject pooledObj = command.GeniePool.Get();
            if (pooledObj.Counter == 0)
                pooledObj.Configure(command.SchemaBuilder, this.Context);

            var grpc = MockPartyCreator.GetParty();
            grpc.Request.Info = pooledObj.Client.ToString();

            var bytes = Any.Pack(grpc).ToByteArray();
            ZeroMQPooledObject.Send(bytes);

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