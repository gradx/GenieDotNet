using Genie.Actors;
using Genie.Common;
using Proto;
using Proto.Cluster;
using Proto.Cluster.PubSub;
using GrainResponse = Genie.Actors.GrainResponse;


namespace Genie.IngressConsumer.Services
{
    public class ActorService
    {
        public static async Task Start()
        {
            Console.WriteLine("Starting Actor System");

            var context = GenieContext.Build();

            var actorSystem = ActorUtils.JoinActorSystem<EventGrain>(context.GenieContext.Actor.ClusterName,
                context.GenieContext.Actor.ConsulProvider, ActorUtils.GetClusterKind<EventGrain>(), ActorGrainReflection.Descriptor);


            await actorSystem
                .Cluster()
                .StartMemberAsync();

            var cluster = actorSystem.Cluster();
            cluster.Gossip.SetState("server-state", new PID("help", "me"));

            cluster.System.EventStream.Subscribe(e =>
            {
                //if (e is not GossipUpdate)
                //    Console.WriteLine($@"Server EventStream Got message for {e}");
            });

            cluster.Subscribe("my-topic", context =>
            {
                //Console.WriteLine($@"Host received Topic: {context.Message}");
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();


            Console.WriteLine("Waiting for instructions");

            while (Console.ReadLine() != null)
            {
                var pid = cluster.System.Root.Get<PID>();

                var getState = cluster.Gossip.GetState<PID>("my-state").GetAwaiter().GetResult();

                cluster.System.EventStream.Publish(new GrainResponse
                {
                    Level = GrainResponse.Types.ResponseLevel.Info,
                    Message = "Publish from Main Goes to None"
                });

                cluster.Publisher()?.Publish("my-topic", new GrainResponse
                {
                    Level = GrainResponse.Types.ResponseLevel.Info,
                    Message = "Topic from Main Goes To Web Only"
                });

                cluster.MemberList.BroadcastEvent(new GrainResponse
                {
                    Level = GrainResponse.Types.ResponseLevel.Info,
                    Message = "BroadcastEvent from Main Goes To All"
                }, false);
            }
        }
    }
}
