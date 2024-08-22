using Google.Protobuf;
using Mediator;
using Proto;
using Genie.Common;
using Genie.Actors;
using Google.Protobuf.WellKnownTypes;
using Genie.Grpc;
using Microsoft.Extensions.ObjectPool;
using Genie.Common.Web;
using Microsoft.AspNetCore.Http;
using Proto.Remote;
using Genie.Common.Performance;

namespace Genie.Actors;

public record ActorCommand(ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget, HttpContext? HttpContext) : IRequest<GrainResponse?>;

public class ActorCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<ActorCommand, GrainResponse?>
{
    public async ValueTask<GrainResponse?> Handle(ActorCommand command, CancellationToken cancellationToken)
    {
        var grpc = MockPartyCreator.GetParty();
        grpc.Party.CosmosBase.Identifier.Id = "Help";

        return await HandleCommand(grpc, command, cancellationToken);
    }

    public static async ValueTask<GrainResponse?> HandleCommand(Google.Protobuf.IMessage partyRequest, ActorCommand command, CancellationToken cancellationToken)
    {
        //var reader = MaxMindDbSupport.Instance;

        //var ip = IPAddress.Parse("73.82.251.128");
        //var data = reader.Find<Dictionary<string, object>>(ip);

        // command.HttpContext.Connection.RemoteIpAddress?.ToString()
        //grpc.Request.IpAddressSource
        //grpc.Request.IpAddressDestination

        var pooledObj = command.GeniePool.Get();
        pooledObj.Counter++;

        var response = await ActorUtils.InitiateActor(command.ActorSystem, new GrainRequest
        {
            Key = partyRequest.GetType().Name,
            Request = new StatusRequest
            {
                Topic = pooledObj.EventChannel,
                Offset = pooledObj.Counter
            },
            Value = Any.Pack(partyRequest),
            Timestamp = DateTime.UtcNow.ToTimestamp(),

        }, command.FireAndForget, cancellationToken);

        //if(pooledObj.Counter > 100000)
        //{
        //    _ = Task.Run(async () =>
        //    {
        //        await ActorUtils.InitiateActor(command.ActorSystem, new GrainRequest
        //        {
        //            Key = "Shutdown",
        //            Request = new StatusRequest
        //            {
        //                Topic = pooledObj.EventChannel,
        //                Offset = pooledObj.Counter
        //            },
        //            Timestamp = DateTime.UtcNow.ToTimestamp(),

        //        }, command.FireAndForget, cancellationToken);
        //    });

        //    pooledObj.EventChannel = Guid.NewGuid().ToString("N");
        //    pooledObj.Counter = 0;
        //}


        command.GeniePool.Return(pooledObj);

        return response;
    }
}
