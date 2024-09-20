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

namespace Genie.Adapters.Brokers.NATS;

public record NatsCommand(ObjectPool<NatsPooledObject> GeniePool, SchemaBuilder SchemaBuilder, ILogger<Exception> Logger, bool FireAndForget) : IRequest<Unit>;

public class NatsCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<NatsCommand, Unit>
{
    public async ValueTask<Unit> Handle(NatsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            NatsPooledObject pooledObj = command.GeniePool.Get();

            if (pooledObj.Counter == 0)
                pooledObj.Configure(command.SchemaBuilder, this.Context);

            var grpc = MockPartyCreator.GetParty();


            grpc.Request.Info = pooledObj.EventChannel;

            var bytes = Any.Pack(grpc).ToByteArray();


            await pooledObj.NatsConnection.PublishAsync<byte[]>(subject: "Genie", data: bytes, cancellationToken: cancellationToken);

            var success = command.FireAndForget || pooledObj.ReceiveSignal.WaitOne(30000);

            EventTaskJob? result = null;

            if (!command.FireAndForget)
                result = pooledObj.Result;

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