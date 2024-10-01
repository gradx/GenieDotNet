using Genie.Extensions.Genius.Commands;
using Genie.Grpc;
using Genie.Utils;
using Grpc.Core;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Proto;

namespace Genie.Extensions.Genius;

public class GeniusEventRPCService(ObjectPool<GeniePooledObject> geniePool, ActorSystem actorSystem, IMediator mediator) : GeniusEventRPC.GeniusEventRPCBase
{
    readonly IMediator mediator = mediator;


    public override async Task EventPoll(IAsyncStreamReader<GeniusEventPollRequest> request, IServerStreamWriter<GeniusEventPollResponse> response, ServerCallContext context)
    {
        var cmd = new GeniusCommand(request, response, context, geniePool, actorSystem, false);
        await mediator.Send(cmd);

    }

    public override async Task Process(IAsyncStreamReader<GeniusEventRequest> request, IServerStreamWriter<GeniusEventResponse> response, ServerCallContext context)
    {
        var cmd = new HashedGeniusCommand(request, response, context, geniePool, actorSystem, false);
        await mediator.Send(cmd);
    }
}