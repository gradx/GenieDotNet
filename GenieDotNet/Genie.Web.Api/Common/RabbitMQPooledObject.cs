using Chr.Avro.Abstract;
using Genie.Common.Utils;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Genie.Common;
using Genie.Common.Adapters.RabbitMQ;
using Genie.Common.Types;
using Genie.Common.Web;

namespace Genie.Web.Api.Common;

public class RabbitMQPooledObject : GeniePooledObject
{
    public IConnection? Connect { get; set; }
    public IModel? Ingress { get; set; }
    public IModel? Events { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);
    private EventingBasicConsumer? Consumer;

    public EventTaskJob? Result { get; set; }


    public void Configure(SchemaBuilder schemaBuilder, GenieContext genieContext)
    {
        ReceiveSignal = new(false);
        this.Connect = RabbitUtils.GetConnection(genieContext.Rabbit, false);
        this.Ingress = this.Connect.CreateModel();
        this.Events = this.Connect.CreateModel();

        this.Ingress.ExchangeDeclare(genieContext.Rabbit.Exchange, ExchangeType.Direct);
        this.Ingress.QueueDeclare(genieContext.Rabbit.Queue, false, false, false, null);
        this.Ingress.QueueBind(genieContext.Rabbit.Queue, genieContext.Rabbit.Exchange, genieContext.Rabbit.RoutingKey, null);

        this.Events.ExchangeDeclare(this.EventChannel, ExchangeType.Direct);
        this.Events.QueueDeclare(this.EventChannel, false, false, false, null);
        this.Events.QueueBind(this.EventChannel, this.EventChannel, genieContext.Rabbit.RoutingKey, null);

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