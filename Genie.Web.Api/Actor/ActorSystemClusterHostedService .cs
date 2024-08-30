using Genie.Actors;
using Proto;
using Proto.Cluster;

namespace Genie.Web.Api.Actor;

public class ActorSystemClusterHostedService : IHostedService
{
    private readonly ActorSystem actorSystem;

    public ActorSystemClusterHostedService(ActorSystem actorSystem)
    {
        this.actorSystem = actorSystem;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await actorSystem
            .Cluster()
            .StartMemberAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await actorSystem
            .Cluster()
            .ShutdownAsync();
    }
}