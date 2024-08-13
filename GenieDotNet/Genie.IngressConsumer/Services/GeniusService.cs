using Proto.Cluster;
using Proto;
using Proto.Cluster.PubSub;
using Genie.Common;
using Genie.Actors;
using Genie.Extensions.Genius;
using System.Security.Principal;
using ZstdSharp.Unsafe;

namespace Genie.IngressConsumer.Services;

public class GeniusService
{
    public static async Task Start()
    {
        Console.WriteLine("Starting Genius System");

        var context = GenieContext.Build();


        // Genius Grain
        var geniusSystem = ActorUtils.JoinActorSystem<GeniusGrain>(context.GenieContext.Actor.ClusterName,
            context.GenieContext.Actor.ConsulProvider, GeniusServiceActor.GetClusterKind((ctx, identity) => new GeniusGrain(ctx, identity)), 
            GeniusGrainReflection.Descriptor);

        await geniusSystem
            .Cluster()
            .StartMemberAsync();

        // Genie Grain
        var actorSystem = ActorUtils.JoinActorSystem<EventGrain>(context.GenieContext.Actor.ClusterName,
            context.GenieContext.Actor.ConsulProvider, ActorUtils.GetClusterKind<EventGrain>(),
            ActorGrainReflection.Descriptor);

        await actorSystem
            .Cluster()
            .StartMemberAsync();


        //var cluster = geniusSystem.Cluster();
        //cluster.Gossip.SetState("server-state", new PID("help", "me"));

        //cluster.System.EventStream.Subscribe(e =>
        //{
        //    //if (e is not GossipUpdate)
        //    //    Console.WriteLine($@"Server EventStream Got message for {e}");
        //});

        //cluster.Subscribe("my-topic", context =>
        //{
        //    //Console.WriteLine($@"Host received Topic: {context.Message}");
        //    return Task.CompletedTask;
        //}).GetAwaiter().GetResult();


        Console.WriteLine("Waiting for instructions");

        while (Console.ReadLine() != null)
        {

        }
    }
}
