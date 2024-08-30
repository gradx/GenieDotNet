using Confluent.Kafka;
using Elastic.Clients.Elasticsearch.Nodes;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Proto;
using Proto.Cluster;
using ZLogger;

namespace Genie.Actors;



public class EventGrain : GrainServiceBase
{
    static public IConfigurationRoot? configuration { get; set; }
    private readonly ClusterIdentity clusterIdentity;
    private ILogger Logger { get; set; }
    private int processCount = 0;
    private readonly Dictionary<int, GrainResponse> offsets = [];
    private readonly GenieContext genieContext;
    private readonly DefaultObjectPool<PostGisPooledObject> pool;
    private readonly CounterConsoleLogger counterLogger = new();


    public EventGrain(IContext context, ClusterIdentity clusterIdentity) : base(context)
    {
        this.clusterIdentity = clusterIdentity;

        //Console.WriteLine($"{clusterIdentity.Identity}");

        // TODO: Move to Kafka Topic and dedicated logger
        var factory = ZloggerFactory.GetAvroFactory(@"C:\temp\logs");
        Logger = factory.CreateLogger("Program");

        genieContext = GenieContext.Build().GenieContext;

        pool = new(new DefaultPooledObjectPolicy<PostGisPooledObject>());


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
        if (offsets.TryGetValue(request.Offset, out GrainResponse? resp))
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

            Console.WriteLine($@"Shutting down {request.Request.Topic} Offset {request.Request.Offset}");
            this.System.Root.Stop(this.Context.Self);
            this.Context.Stop(this.Context.Self);
            //this.Context.Poison(this.Context.Self);
            //this.Context.Stop(this.Context.Self);

            GC.Collect();
            return Task.FromResult(new GrainResponse { Level = GrainResponse.Types.ResponseLevel.Processed, Message = "Shutdown" });
        }

        //if (request.Request.Offset >= 5000)
        //    Console.WriteLine($@"Request {request.Request.Topic} Offset {request.Request.Offset} reached");

        if (genieContext.Simple || request.Request.Offset + 1 < processCount)
            return Task.FromResult(new GrainResponse { Level = GrainResponse.Types.ResponseLevel.Errored, Exception = "Dupe" });

        counterLogger.Process();
        //var state = this.Cluster.Gossip.GetState<PID>("my-state").GetAwaiter().GetResult();

        //this.Cluster.MemberList.BroadcastEvent(new GrainResponse { Message = $@"BroadcastEvent from Grain {appendMsg}" }, false);
        //this.Cluster.System.EventStream.Publish($@"Publish from Grain {appendMsg}");

        //Console.WriteLine($@"{this.Context.ClusterIdentity()?.Identity} initiating Topic: " + request.Timestamp.ToString() + "_" + processCount.ToString());
        //this.Cluster.Publisher().Publish("my-topic", new GrainResponse { Message = $@"Topic from Grain {appendMsg}" });


        // TODO: Add Avro support
        IMessage message = request.Key switch
        {
            "PartyRequest" => request.Value.Unpack<Grpc.PartyRequest>(),
            "PartyBenchmarkRequest" => request.Value.Unpack<Grpc.PartyBenchmarkRequest>(),
            _ => throw new TypeLoadException($"Type not mapped: {request.Key}")
        };


        var result = EventTask.Process(genieContext, message,
            Logger, pool, new CancellationToken()).GetAwaiter().GetResult();


        //string appendMsg = $@"{_clusterIdentity.Identity} with offset {request.Request.Offset} and processCount {processCount}";
        //string respMsg = $"{appendMsg} and result {result}";
        //Console.WriteLine(respMsg);


        var grainResp = new GrainResponse { Response = Any.Pack(result) };
        //Offsets.Add(request.Offset, grainResp);

        return Task.FromResult(grainResp);
    }
}