using Chr.Avro.Abstract;
using Confluent.SchemaRegistry.Serdes;
using Genie.Common.Adapters;
using Genie.Common;
using Genie.Web.Api.Common;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using System.Net;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;

namespace Genie.Web.Api.Mediator.Commands;

public record ActiveMQCommand(ObjectPool<ActiveMQPooledObject> GeniePool, SchemaBuilder SchemaBuilder, ILogger<Exception> Logger) : IRequest<HttpStatusCode>;

public class ActiveMQCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<ActiveMQCommand, HttpStatusCode>
{
    public async ValueTask<HttpStatusCode> Handle(ActiveMQCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var grpc = MockPartyCreator.GetParty();
            var partyRequest = CosmosAdapter.ToCosmos(grpc);
            ActiveMQPooledObject pooledObj = command.GeniePool.Get();

            var bytes = Any.Pack(grpc).ToByteArray();

            if (pooledObj.Counter == 0)
                pooledObj.Configure(this.Context);

            var result = pooledObj.Send(bytes);

            pooledObj.Counter++;
            command.GeniePool.Return(pooledObj);

            if (result != null)
                return await Task.FromResult(HttpStatusCode.OK);
        }
        catch(Exception ex)
        {
            command.Logger.LogError(ex, "ActiveMQCommandHandler");
        }

        throw new BadHttpRequestException("Server response was invalid");
    }
}