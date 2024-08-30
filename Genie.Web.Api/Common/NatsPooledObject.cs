using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Types;
using Genie.Common.Utils;
using NATS.Client.Core;

namespace Genie.Web.Api.Common;

public class NatsPooledObject : GeniePooledObject
{
    public NatsConnection NatsConnection { get; set; }

    public BinaryDeserializer<EventTaskJob> Deserializer { get; set; }

    public EventTaskJob? Result { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);


    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {
        NatsConnection = new NatsConnection();

        _ = Task.Run(async () => {
            await foreach (var msg in NatsConnection.SubscribeAsync<byte[]>(subject: EventChannel))
            {
                Result = Deserialize(msg.Data!);
                ReceiveSignal.Set();
            }
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