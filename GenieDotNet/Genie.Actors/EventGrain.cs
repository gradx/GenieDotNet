
using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Genie.Common;
using Genie.Common.Adapters;
using Genie.Common.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Gossip;
using Proto.Cluster.PubSub;
using ZLogger;
using ZLogger.Providers;

namespace Genie.Actors;

public class EventGrain : GrainServiceBase
{
    static public IConfigurationRoot? Configuration { get; set; }
    private readonly ClusterIdentity _clusterIdentity;
    private ILogger Logger { get; set; }
    private int processCount = 0;
    private readonly Dictionary<int, GrainResponse> Offsets = [];
    private readonly GenieContext genieContext;


    public EventGrain(IContext context, ClusterIdentity clusterIdentity) : base(context)
    {
        _clusterIdentity = clusterIdentity;

        //Console.WriteLine($"{_clusterIdentity.Identity}");

        // TODO: Move to Kafka Topic and dedicated logger
        var factory = ZloggerFactory.GetAvroFactory(@"C:\temp\logs");
        Logger = factory.CreateLogger("Program");

        genieContext = GenieContext.Build().GenieContext;


        //this.Cluster.System.EventStream.Subscribe(e =>
        //{
        //    //if (e is not GossipUpdate)
        //    //    Console.WriteLine($@"Grain EventStream Got message for {e}");
        //});

        //var pid = this.Cluster.Subscribe("my-topic", context =>
        //{
        //    //Console.WriteLine($@"Grain {clusterIdentity.Identity} received Topic: {context.Message}");
        //    return Task.CompletedTask;
        //}
        //);
    }

    public override Task<GrainResponse> Status(StatusRequest request)
    {
        if (Offsets.TryGetValue(request.Offset, out GrainResponse? resp))
            return Task.FromResult(resp);
        else
            return Task.FromResult(new GrainResponse
            {
                Level = GrainResponse.Types.ResponseLevel.Errored,
                Exception = "Not Found"
            });
    }

    public override Task<GrainResponse> Process(GrainRequest request)
    {
        processCount++;

        if (request.Key == "Shutdown")
        {
            this.Context.Stop(this.Context.Self);
            return Task.FromResult(new GrainResponse { Level = GrainResponse.Types.ResponseLevel.Processed, Message = "Shutdown" });
        }
        
        if (request.Request.Offset + 1 < processCount)
            return Task.FromResult(new GrainResponse { Level = GrainResponse.Types.ResponseLevel.Errored, Exception = "Dupe" });

        //var state = this.Cluster.Gossip.GetState<PID>("my-state").GetAwaiter().GetResult();

        //this.Cluster.MemberList.BroadcastEvent(new GrainResponse { Message = $@"BroadcastEvent from Grain {appendMsg}" }, false);
        //this.Cluster.System.EventStream.Publish($@"Publish from Grain {appendMsg}");

        //Console.WriteLine($@"{this.Context.ClusterIdentity()?.Identity} initiating Topic: " + request.Timestamp.ToString() + "_" + processCount.ToString());
        //this.Cluster.Publisher().Publish("my-topic", new GrainResponse { Message = $@"Topic from Grain {appendMsg}" });


        // TODO: Add Avro support
        IMessage message = request.Key switch
        {
            "PartyRequest" => request.Value.Unpack<Grpc.PartyRequest>(), //new ProtobufDeserializer<Grpc.PartyRequest>().DeserializeAsync(request.Value.ToArray(), false, serContext).GetAwaiter().GetResult(),
            _ => throw new TypeLoadException($"Type not mapped: {request.Key}")
        };


        var result = EventTask.Process(genieContext, message,
            Logger, new CancellationToken()).GetAwaiter().GetResult();


        string appendMsg = $@"{_clusterIdentity.Identity} with offset {request.Request.Offset} and processCount {processCount}";
        string respMsg = $"{appendMsg} and result {result}";
        //Console.WriteLine(respMsg);

        var grainResp = new GrainResponse { Message = respMsg, Response = Any.Pack(result) };
        //Offsets.Add(request.Offset, grainResp);

        return Task.FromResult(grainResp);
    }
}