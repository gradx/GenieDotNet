using Mediator;
using Proto;
using Genie.Common;
using Genie.Actors;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.ObjectPool;
using Genie.Common.Web;
using Genie.Extensions.Genius;
using Proto.Cluster;
using Genie.Grpc;
using Grpc.Core;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace Genie.Extensions.Commands;

public record GeniusCommand(IAsyncStreamReader<GeniusEventPollRequest>? Request, IServerStreamWriter<GeniusEventPollResponse>? Response, ServerCallContext? Context, ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget) : IRequest;

public class GeniusCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<GeniusCommand>
{
    public async ValueTask<Unit> Handle(GeniusCommand command, CancellationToken cancellationToken)
    {

        // Inheriting from ActorCommand causes it to route to only ActorCommandHandler
        // MissingMessageHandlerException: No handler registered for message type: Genie.Web.Api.Mediator.Commands.GeniusCommand
        _ = await ActorCommandHandler.HandleCommand(new PartyRequest(), new ActorCommand(command.GeniePool, command.ActorSystem, command.FireAndForget, null), cancellationToken);


        var grpc = MockPartyCreator.GetParty();
        grpc.Party.CosmosBase.Identifier.Id = "Help";

        // command.HttpContext.Connection.RemoteIpAddress?.ToString()
        //grpc.Request.IpAddressSource
        //grpc.Request.IpAddressDestination

        var pooledObj = command.GeniePool.Get();

        var response = await InitiateActor(command.ActorSystem, new GeniusRequest
        {
            Key = grpc.GetType().Name,
            Value = Any.Pack(grpc),
            Timestamp = DateTime.UtcNow.ToTimestamp(),
            Request = new Genius.StatusRequest
            {
                Topic = pooledObj.EventChannel,
                Offset = pooledObj.Counter
            },


        }, command.FireAndForget, new CancellationToken());

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        using CancellationTokenSource cts = new();

        do
        {
            if (command?.Request?.Current == null)
                continue;

            await command!.Response!.WriteAsync(new GeniusEventPollResponse
            {
                Job = new GeniusEventTaskJob
                {
                     Job = response?.Message
                },
                Response = new BaseResponse { Success = true }
            }, cts.Token);


        } while (await command!.Request!.MoveNext());


        return new Unit();
    }

    private static async Task<GeniusResponse?> InitiateActor(ActorSystem actorSystem, GeniusRequest request, bool fireAndForget, CancellationToken cancellationToken)
    {
        var grainClient = actorSystem.Cluster().GetGeniusService("help");
        if (fireAndForget)
        {
            _ = grainClient.Process(request, cancellationToken);
            return null;
        }
        else
            return await grainClient.Process(request, cancellationToken);
    }
}
