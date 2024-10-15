using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Adapters.Serializers.Avro;
using Genie.Core;
using NetMQ;
using NetMQ.Sockets;

namespace Genie.Adapters.Brokers.ZeroMQ;
public class ZeroMQPooledObject<T> : GeniePooledObject
{
    private static DealerSocket Server { get; set; } = new DealerSocket("@tcp://127.0.0.1:5555");
    public int? Client { get; set; }

    public BinaryDeserializer<T> Deserializer { get; set; }

    public RoutingKey RoutingKey { get; set; }

    public AutoResetEvent ReceiveSignal = new(false);
    public T? Result { get; set; }

    private readonly static Mutex mutex = new(false, "ZeroMQ");

    public static void Send(byte[] bytes)
    {
        //Mutex mapMutex = new Mutex(false, "OvertureMaps");
        //mapMutex.WaitOne();
        mutex.WaitOne();
        Server.SendFrame(bytes);
        mutex.ReleaseMutex();
    }


    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {
        //var server = new DealerSocket("@tcp://127.0.0.1:5555");
        var client = new DealerSocket(); // ">tcp://127.0.0.1:7777"
        Client = client.BindRandomPort("tcp://127.0.0.1");

        //Server = server;

        new NetMQProactor(client, (socket, message) =>
        {

            Result = Deserialize(message.First.Buffer);
            this.ReceiveSignal.Set();

            //var frames = message.ToArray();
            //foreach (var m in message)
            //{
            //    Result = Deserialize(m.Buffer);
            //}
        });


        var schema = schemaBuilder.BuildSchema<T>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<T>(schema);

    }


    public T Deserialize(byte[] help)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(help);
        return Deserializer(ref reader);
    }
}