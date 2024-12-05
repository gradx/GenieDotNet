using Chr.Avro.Abstract;
using Chr.Avro.Serialization;
using Confluent.Kafka;
using Genie.Adapters.Serializers.Avro;
using Genie.Core;
using Microsoft.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Genie.Adapters.Brokers.RabbitMQ;
public class RabbitMQPooledObject<T,K> : GeniePooledObject
{
    public static IConnection? Connect { get; set; }
    public IChannel? Ingress { get; set; }
    public IChannel? Events { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);
    //private EventingBasicConsumer? Consumer;
    public T? Result { get; set; }
    private static readonly RecyclableMemoryStreamManager manager = new();
    private ISerializer<K> Serializer { get; set; }
    private BinaryDeserializer<T> Deserializer { get; set; }
    private AsyncEventingBasicConsumer AsyncHandler { get; set; }

    public void Reset()
    {
        Connect?.Dispose();
        Ingress?.Dispose();
        Events?.Dispose();

        Connect = null;
        Ingress = null;
        Events = null;
        ReceiveSignal = new(false);
        //Consumer = null;
    }

    public async Task Configure(SchemaBuilder schemaBuilder, GenieContext genieContext, CancellationToken cancellationToken)
    {
        ReceiveSignal = new(false);

        var args = new Dictionary<string, object>
        {
            { "x-max-length", 10000 }
        };

        RabbitMQPooledObject<T,K>.Connect = RabbitUtils.Instance;

        this.Ingress = await RabbitMQPooledObject<T, K>.Connect.CreateChannelAsync(cancellationToken: cancellationToken);
        this.Events =  await RabbitMQPooledObject<T, K>.Connect.CreateChannelAsync(cancellationToken: cancellationToken);

        await Events.ExchangeDeclareAsync(this.EventChannel, ExchangeType.Direct, cancellationToken: cancellationToken);
        await Events.QueueDeclareAsync(this.EventChannel, false, false, false, args, cancellationToken: cancellationToken);
        await Events.QueueBindAsync(this.EventChannel, this.EventChannel, genieContext.RabbitMQ.RoutingKey, cancellationToken: cancellationToken);

        AsyncHandler = new AsyncEventingBasicConsumer(this.Events);
        var result = await this.Events.BasicConsumeAsync(this.EventChannel, true, AsyncHandler, cancellationToken);
        var schema = schemaBuilder.BuildSchema<T>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        Deserializer = deserializerBuilder.BuildDelegate<T>(schema);

        AsyncHandler.ReceivedAsync += EventReceived;
    }

    private Task EventReceived(object sender, BasicDeliverEventArgs @event)
    {
        var reader = new Chr.Avro.Serialization.BinaryReader(@event.Body.ToArray());
        Result = Deserializer(ref reader);
        ReceiveSignal.Set();

        return Task.CompletedTask;
    }
}