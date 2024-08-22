using Chr.Avro.Abstract;
using Genie.Common.Utils;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Genie.Common;
using Genie.Common.Adapters.RabbitMQ;
using Genie.Common.Types;
using Genie.Common.Performance;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Genie.Web.Api.Common;

public class RabbitMQPooledObject : GeniePooledObject
{
    public IConnection? Connect { get; set; }
    public IModel? Ingress { get; set; }
    public IModel? Events { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);
    private EventingBasicConsumer? Consumer;

    public EventTaskJob? Result { get; set; }

    public void Reset()
    {
        Connect?.Close();
        Connect?.Dispose();
        Ingress?.Close();
        Ingress?.Dispose();
        Events?.Close();
        Events?.Dispose();

        Connect = null;
        Ingress = null;
        Events = null;
        ReceiveSignal = new(false);
        Consumer = null;
    }

    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {
        ReceiveSignal = new(false);

        var args = new Dictionary<string, object>();
        args.Add("x-max-length", 10000);

        this.Connect = RabbitUtils.GetConnection(genieContext.RabbitMQ, false);
        this.Ingress = this.Connect.CreateModel();
        this.Events = this.Connect.CreateModel();

        this.Ingress.ExchangeDeclare(genieContext.RabbitMQ.Exchange, ExchangeType.Direct);
        this.Ingress.QueueDeclare(genieContext.RabbitMQ.Queue, false, false, false, args);
        this.Ingress.QueueBind(genieContext.RabbitMQ.Queue, genieContext.RabbitMQ.Exchange, genieContext.RabbitMQ.RoutingKey, null);

        this.Events.ExchangeDeclare(this.EventChannel, ExchangeType.Direct);
        this.Events.QueueDeclare(this.EventChannel, false, false, false, args);
        this.Events.QueueBind(this.EventChannel, this.EventChannel, genieContext.RabbitMQ.RoutingKey, null);

        Consumer = new EventingBasicConsumer(this.Events);

        var schema = schemaBuilder.BuildSchema<EventTaskJob>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        var binaryDeserializer = deserializerBuilder.BuildDelegate<EventTaskJob>(schema);

        Consumer.Received += (sender, ea) =>
        {
            var reader = new Chr.Avro.Serialization.BinaryReader(ea.Body.ToArray());
            Result = binaryDeserializer(ref reader);

            this.ReceiveSignal.Set();
        };

        this.Events.BasicConsume(this.EventChannel, true, Consumer);
    }
}