using Adaptive.Aeron;
using Adaptive.Aeron.LogBuffer;
using Adaptive.Agrona;
using Adaptive.Agrona.Concurrent;
using Adaptive.Cluster.Client;
using Adaptive.Cluster.Codecs;
using Adaptive.Cluster.Service;
using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Utils;

namespace Genie.Adapters.Brokers.Aeron;
internal class MessageListener : IEgressListener
{
    public void OnMessage(long clusterSessionId, long timestampMs, IDirectBuffer buffer, int offset, int length, Header header)
    {
        Console.WriteLine($"OnMessage: sessionId={clusterSessionId}, timestamp={timestampMs}, length={length}");

        Console.WriteLine("Received Message: " + buffer.GetStringWithoutLengthUtf8(offset, length));
    }

    public void OnSessionEvent(long correlationId, long clusterSessionId, long leadershipTermId, int leaderMemberId, EventCode code, string detail)
    {
        Console.WriteLine($"Session Event:  leadershipTermId={leadershipTermId}, leaderMemberId={leaderMemberId}, code={code}, detail={detail}");
    }

    public void OnNewLeader(long clusterSessionId, long leadershipTermId, int leaderMemberId, string memberEndpoints)
    {
        Console.WriteLine($"New Leader:  leadershipTermId={leadershipTermId}, leaderMemberId={leaderMemberId}, memberEndpoints={memberEndpoints}");
    }

    public void OnAdminResponse(long clusterSessionId, long correlationId, AdminRequestType requestType,
        AdminResponseCode responseCode, string message, IDirectBuffer payload, int payloadOffset, int payloadLength)
    {
        Console.WriteLine($"OnAdminResponse:  clusterSessionId={clusterSessionId}, correlationId={correlationId}, requestType={requestType}, responseCode={responseCode}");
    }
}

public class EchoService : IClusteredService
{
    private ICluster _cluster;

    public void OnStart(ICluster cluster, Image snapshotImage)
    {
        Console.WriteLine("OnStart");
        _cluster = cluster;
    }

    public void OnSessionOpen(IClientSession session, long timestampMs)
    {
        Console.WriteLine($"OnSessionOpen: sessionId={session.Id}, timestamp={timestampMs}");
    }

    public void OnSessionClose(IClientSession session, long timestampMs, CloseReason closeReason)
    {
        Console.WriteLine($"OnSessionClose: sessionId={session.Id}, timestamp={timestampMs}");
    }

    public void OnSessionMessage(IClientSession session, long timestampMs, IDirectBuffer buffer, int offset, int length, Header header)
    {
        Console.WriteLine($"OnSessionMessage: sessionId={session.Id}, timestamp={timestampMs}, length={length}");

        Console.WriteLine("Received Message: " + buffer.GetStringWithoutLengthUtf8(offset, length));

        while (session.Offer(buffer, offset, length) <= 0)
        {
            _cluster.IdleStrategy().Idle();
        }
    }

    public void OnTimerEvent(long correlationId, long timestampMs)
    {
        Console.WriteLine($"OnTimerEvent: correlationId={correlationId}, timestamp={timestampMs}");
    }

    public void OnTakeSnapshot(ExclusivePublication snapshotPublication)
    {
        Console.WriteLine("OnTakeSnapshot");
    }

    public void OnRoleChange(ClusterRole newRole)
    {
        Console.WriteLine($"OnRoleChange: newRole={newRole}");
    }

    public void OnTerminate(ICluster cluster)
    {
        Console.WriteLine("OnTerminate");
    }

    public void OnNewLeadershipTermEvent(
        long leadershipTermId,
        long logPosition,
        long timestamp,
        long termBaseLogPosition,
        int leaderMemberId,
        int logSessionId,
        ClusterTimeUnit timeUnit,
        int appVersion)
    {
        Console.WriteLine($"OnNewLeadershipTerm: leadershipTermId={leadershipTermId}");
    }

    public int DoBackgroundWork(long nowNs)
    {
        return 0;
    }
}

public class AeronServicePooledObject : GeniePooledObject
{
    private static Publication? Publication { get; set; }
    private AeronCluster.Context? ClientContext { get; set; }
    private AeronCluster? Cluster { get; set; }
    private ClusteredServiceContainer.Context? ServerContext { get; set; }
    private ClusteredServiceContainer? ServerContainer { get; set; }

    public BinaryDeserializer<EventTaskJob>? Deserializer { get; set; }

    public EventTaskJob? Result { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);


    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {
        //ServerContext = new ClusteredServiceContainer.Context()
        //    .ClusteredService(new EchoService());

        //ServerContainer = ClusteredServiceContainer.Launch(ServerContext);

        ClientContext = new AeronCluster.Context()
                        .IngressChannel("aeron:udp?endpoint=localhost:9010")
                        .EgressListener(new MessageListener());
        Cluster = AeronCluster.Connect(ClientContext);



        var schema = schemaBuilder.BuildSchema<EventTaskJob>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<EventTaskJob>(schema);
    }


    public EventTaskJob Deserialize(byte[] help)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(help);
        return Deserializer!(ref reader);
    }

    public void Send(byte[] bytes)
    {
        var buffer = new UnsafeBuffer(new byte[100]);
        var len = buffer.PutStringWithoutLengthUtf8(0, "Hello World!");
        Cluster?.Offer(buffer, 0, len);
    }
}