using Proto.Cluster.PartitionActivator;
using Proto.Cluster;
using Proto.Remote.GrpcNet;
using Proto;
using Proto.Remote;
using Proto.Cluster.Testing;
using Proto.Cluster.Consul;
using Google.Protobuf.Reflection;


namespace Genie.Actors;

public class GeniusActorSystem
{
    public ActorSystem? System { get; set; }
}

public abstract class ActorUtils
{
    public static async Task<GrainResponse?> InitiateActor(ActorSystem actorSystem, GrainRequest request, bool fireAndForget, CancellationToken cancellationToken)
    {
        var grainClient = actorSystem.Cluster().GetGrainService(request.Request.Topic);
        if (fireAndForget)
        {
            _ = grainClient.Process(request, cancellationToken);
            return null;
        }
        else
            return await grainClient.Process(request, cancellationToken);
    }

    public static ActorSystem JoinActorSystem<T>(string clusterName, string host, ClusterKind clusterKind, FileDescriptor descriptor) 
    {
        var actorSystem = new ActorSystem(new ActorSystemConfig { SharedFutures = true })
            .WithRemote(GrpcNetRemoteConfig.BindTo(host)
            .WithProtoMessages(
                descriptor))
            .WithCluster(ClusterConfig
                .Setup(clusterName, GetConsulProvider(), new PartitionActivatorLookup())
                        .WithClusterKind(clusterKind)
        );

        return actorSystem;
    }

    public static ActorSystem JoinActorSystem(string clusterName, string host, FileDescriptor[] descriptor)
    {
        var actorSystem = new ActorSystem(new ActorSystemConfig { SharedFutures = true })
            .WithRemote(GrpcNetRemoteConfig.BindTo(host)
            .WithProtoMessages(descriptor))
            .WithCluster(ClusterConfig.Setup(clusterName, GetConsulProvider(), new PartitionActivatorLookup()));

        return actorSystem;
    }
    public static ClusterKind GetClusterKind<T>()
        where T : GrainServiceBase => GrainServiceActor.GetClusterKind((ctx, identity) => (T)System.Activator.CreateInstance(typeof(T), ctx, identity)!);

    private static ConsulProvider GetConsulProvider() => new (new ConsulProviderConfig());

    private static TestProvider TestProvider() => new (new TestProviderOptions(), new InMemAgent());
}