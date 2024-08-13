using Genie.Actors;
using Genie.Common;
using Genie.Extensions.Genius;
using Proto.Cluster;


//const string baseUrl = "https://luxur.ai:5003";

var context = GenieContext.Build().GenieContext;

var geniusSystem = ActorUtils.JoinActorSystem(context.Actor.ClusterName, context.Actor.ConsulProvider, [GeniusGrainReflection.Descriptor]);

await geniusSystem.Cluster().StartMemberAsync();



Console.WriteLine("Wait for Instructions");
while (Console.ReadLine() != null)
{
    var geniusClient = geniusSystem.Cluster().GetGeniusService("help");
    await geniusClient.Process(new GeniusRequest(), new CancellationToken());
}
