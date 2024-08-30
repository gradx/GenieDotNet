using Adaptive.Aeron;
using Adaptive.Agrona.Concurrent;
using Adaptive.Agrona;
using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using System.Net.Sockets;
using System.Net;

namespace Genie.Web.Api.Common;

public class AeronPooledObject : GeniePooledObject
{
    private static Publication? Publication { get; set; }
    public AeronSubscription? Subscription { get; set; }

    public BinaryDeserializer<EventTaskJob> Deserializer { get; set; }

    public EventTaskJob? Result { get; set; }

    private Aeron.Context AeronContext { get; set; }
    private Aeron Aeron { get; set; }

    public string SubscriptionUrl { get; set; }

    private Mutex mutex = new Mutex(false, "AeronPooledObject");


    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {

        AeronContext = new Aeron.Context();
        Aeron = Aeron.Connect(AeronContext);

        if (Publication == null)
        {
            mutex.WaitOne();
            Publication = Aeron.AddPublication("aeron:udp?endpoint=localhost:40123", 10);
            mutex.ReleaseMutex();
        }
            

        var port = GetRandomPort();
        Subscription = AeronUtils.SetupSubscriber(Aeron, $@"aeron:udp?endpoint=localhost:{port}", 10);

        var schema = schemaBuilder.BuildSchema<EventTaskJob>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<EventTaskJob>(schema);
    }

    public static int GetRandomPort()
    {
        TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public EventTaskJob Deserialize(byte[] help)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(help);
        return Deserializer(ref reader);
    }

    public void Send(byte[] bytes)
    {
        var buffer = new UnsafeBuffer(BufferUtil.AllocateDirectAligned(512, BitUtil.CACHE_LINE_LENGTH));
        buffer.PutBytes(0, bytes);

        mutex.WaitOne();
        var result = Publication.Offer(buffer, 0, bytes.Length);
        mutex.ReleaseMutex();

        if (result < 0L)
        {
            switch (result)
            {
                case Publication.BACK_PRESSURED:
                    Console.WriteLine(" Offer failed due to back pressure");
                    break;
                case Publication.NOT_CONNECTED:
                    Console.WriteLine(" Offer failed because publisher is not connected to subscriber");
                    break;
                case Publication.ADMIN_ACTION:
                    Console.WriteLine("Offer failed because of an administration action in the system");
                    break;
                case Publication.CLOSED:
                    Console.WriteLine("Offer failed publication is closed");
                    break;
                default:
                    Console.WriteLine(" Offer failed due to unknown reason");
                    break;
            }
        }
    }
}