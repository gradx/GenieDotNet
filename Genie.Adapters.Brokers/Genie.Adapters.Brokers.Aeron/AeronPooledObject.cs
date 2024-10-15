using Adaptive.Aeron;
using Adaptive.Agrona.Concurrent;
using Adaptive.Agrona;
using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using System.Net.Sockets;
using System.Net;
using Genie.Adapters.Serializers.Avro;
using Genie.Core;


namespace Genie.Adapters.Brokers.Aeron;

public class AeronPooledObject<T> : GeniePooledObject
{
    private static Publication? Publication { get; set; }
    public AeronSubscription? Subscription { get; set; }

    public BinaryDeserializer<T> Deserializer { get; set; }

    public T? Result { get; set; }

    private Adaptive.Aeron.Aeron.Context AeronContext { get; set; }
    private Adaptive.Aeron.Aeron Aeron { get; set; }

    public string SubscriptionUrl { get; set; }

    private readonly Mutex mutex = new(false, "AeronPooledObject");


    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {

        AeronContext = new Adaptive.Aeron.Aeron.Context();
        Aeron = Adaptive.Aeron.Aeron.Connect(AeronContext);

        if (Publication == null)
        {
            mutex.WaitOne();
            Publication = Aeron.AddPublication("aeron:udp?endpoint=localhost:40123", 10);
            mutex.ReleaseMutex();
        }
            

        var port = GetRandomPort();
        Subscription = AeronUtils.SetupSubscriber(Aeron, $@"aeron:udp?endpoint=localhost:{port}", 10);

        var schema = schemaBuilder.BuildSchema<T>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<T>(schema);
    }

    public static int GetRandomPort()
    {
        TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public T Deserialize(byte[] help)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(help);
        return Deserializer(ref reader);
    }

    public void Send(byte[] bytes)
    {
        var buffer = new UnsafeBuffer(BufferUtil.AllocateDirectAligned(512, BitUtil.CACHE_LINE_LENGTH));
        buffer.PutBytes(0, bytes);

        mutex.WaitOne();
        var result = Publication!.Offer(buffer, 0, bytes.Length);
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