using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Common;
using Genie.Common.Types;
using Genie.Common.Utils;
using Genie.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace Genie.Adapters.Brokers.ZeroMQ;
public class ZeroMQPooledObject : GeniePooledObject
{
    private static DealerSocket Server { get; set; } = new DealerSocket("@tcp://127.0.0.1:5555");
    public int? Client { get; set; }

    public BinaryDeserializer<EventTaskJob> Deserializer { get; set; }

    public RoutingKey RoutingKey { get; set; }

    public AutoResetEvent ReceiveSignal = new(false);
    public EventTaskJob? Result { get; set; }

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


        var schema = schemaBuilder.BuildSchema<EventTaskJob>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<EventTaskJob>(schema);
    }



    public EventTaskJob Deserialize(byte[] help)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(help);
        return Deserializer(ref reader);
    }
}