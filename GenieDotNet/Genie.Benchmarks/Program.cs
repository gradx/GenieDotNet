// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Chr.Avro.Confluent;
using Chr.Avro.Serialization;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Genie.Actors;
using Genie.Common;
using Genie.Common.Adapters;
using Genie.Common.Utils;
using Google.Protobuf.WellKnownTypes;
using Proto;
using Proto.Cluster;
using Pulsar.Client.Api;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Genie.Common.Types;
using Chr.Avro.Abstract;
using Genie.Common.Adapters.Kafka;
using Apache.NMS.Util;
using Apache.NMS;





Console.WriteLine("Hello, benchmarks!");
//Console.ReadLine();
var test = new BrokerBenchmarks();
test.ActiveMQ();
//Console.ReadLine();
//Console.WriteLine("Completed");
//Console.ReadLine();


//while (true)
//{
//    Console.WriteLine("Send Request?");
//    Console.ReadLine();
//    test.RabbitMQ();

//    Console.WriteLine("Request Completed");
//}


//Console.ReadLine();

_ = BenchmarkRunner.Run<BrokerBenchmarks>();

public class BrokerBenchmarks
{
    private readonly CachedSchemaRegistryClient schemaRegistry = new(AvroSupport.GetSchemaRegistryConfig());
    private readonly SchemaBuilder schemaBuilder = AvroSupport.GetSchemaBuilder();
    private readonly ActorSystem actorSystem;
    private readonly byte[] requestFile = File.ReadAllBytes(@"C:\temp\partyrequest.grpc");
    private readonly GenieContext context;
    private readonly Chr.Avro.Abstract.Schema schema;
    private readonly Confluent.SchemaRegistry.Serdes.ProtobufSerializer<Genie.Grpc.PartyRequest> protobufSerializer;
    private readonly BinaryDeserializer<EventTaskJob> binaryDeserializer;

    // Pulsar.Net
    private readonly PulsarClient? pulsarClient;
    private readonly Pulsar.Client.Api.IProducer<byte[]>? pulsarProducer;
    
    // Apache Pulsar
    private readonly IPulsarClient? dotPulsarClient;
    private readonly DotPulsar.Abstractions.IProducer<byte[]>? dotPulsarProducer;
    private readonly IConsumer<string, EventTaskJob> kafkaConsumer;
    private readonly IProducer<string, PartyRequest> kafkaProducer;

    // RabbitMQ
    private readonly RabbitMQ.Client.IConnection? rabbitConnect;
    private readonly IModel? rabbitIngress;
    private readonly IModel? rabbitEvents;
    private readonly AutoResetEvent receiveSignal = new(false);

    // Kafka
    private readonly IAdminClient adminClient;

    private readonly bool loadPulsar = false;
    private readonly bool loadRabbit = false;

    public BrokerBenchmarks()
    {
        context = GenieContext.Build().GenieContext;
        adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = context.Kafka.Host }).Build();
        
        actorSystem = ActorUtils.JoinActorSystem(context.Actor.ClusterName, context.Actor.ConsulProvider, [ActorGrainReflection.Descriptor]);

        actorSystem
            .Cluster()
            .StartMemberAsync().GetAwaiter().GetResult();

        actorSystem.Cluster();

        schema = schemaBuilder.BuildSchema<EventTaskJob>();

        protobufSerializer = new Confluent.SchemaRegistry.Serdes.ProtobufSerializer<Genie.Grpc.PartyRequest>(new Confluent.SchemaRegistry.CachedSchemaRegistryClient(AvroSupport.GetSchemaRegistryConfig()));

        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        binaryDeserializer = deserializerBuilder.BuildDelegate<EventTaskJob>(schema);



        if (loadPulsar)
        {
            dotPulsarClient = DotPulsar.PulsarClient.Builder().ServiceUrl(new Uri("pulsar://pulsar:6650")).Build();

            pulsarClient = new PulsarClientBuilder()
            .ServiceUrl("pulsar://pulsar:6650")
            .BuildAsync().GetAwaiter().GetResult();

            pulsarProducer = pulsarClient.NewProducer()
                .Topic(context.Kafka.Ingress)
                .CreateAsync().GetAwaiter().GetResult();


            dotPulsarProducer = dotPulsarClient.NewProducer(DotPulsar.Schema.ByteArray)
                .Topic(context.Kafka.Ingress + "Apache")
                .Create();

        }

        var config = KafkaUtils.GetConfig(context);

        using CancellationTokenSource cts = new();


        var builder = new ConsumerBuilder<string, EventTaskJob>(config);
        builder.SetAvroKeyDeserializer(schemaRegistry);
        builder.SetAvroValueDeserializer(schemaRegistry);
        kafkaConsumer = builder.Build();

        var registryConfig = AvroSupport.GetSchemaRegistryConfig();

        var producerBuilder = new ProducerBuilder<string,PartyRequest>(new ProducerConfig { 
            BootstrapServers = context.Kafka.Host });
        producerBuilder.SetAvroKeySerializer(schemaRegistry,
            $"{typeof(PartyRequest).FullName}-Key", registerAutomatically: AutomaticRegistrationBehavior.Always).GetAwaiter().GetResult();
        producerBuilder.SetAvroValueSerializer(AvroSupport.GetSerializerBuilder(registryConfig, schemaBuilder),
            $"{typeof(PartyRequest).FullName}-Value", registerAutomatically: AutomaticRegistrationBehavior.Always).GetAwaiter().GetResult();
        //Use name as shortcut
        kafkaProducer = producerBuilder.Build();

        
        if (loadRabbit)
        {
            rabbitConnect = GetConnection("guest", "guest", "", "localhost");

            rabbitIngress = rabbitConnect.CreateModel();
            rabbitIngress.ExchangeDeclare(this.context.RabbitMQ.Exchange, ExchangeType.Direct);
            rabbitIngress.QueueDeclare(this.context.RabbitMQ.Queue, false, false, false, null);
            rabbitIngress.QueueBind(this.context.RabbitMQ.Queue, this.context.RabbitMQ.Exchange, this.context.RabbitMQ.RoutingKey, null);

            rabbitEvents = rabbitConnect.CreateModel();
            rabbitEvents.ExchangeDeclare(context.Kafka.Events, ExchangeType.Direct);
            rabbitEvents.QueueDeclare(context.Kafka.Events, false, false, false, null);
            rabbitEvents.QueueBind(context.Kafka.Events, context.Kafka.Events, this.context.RabbitMQ.RoutingKey, null);

            var consumer = new EventingBasicConsumer(rabbitEvents);

            consumer.Received += (sender, ea) =>
            {
                var reader = new Chr.Avro.Serialization.BinaryReader(ea.Body.ToArray());
                var result = binaryDeserializer(ref reader);

                receiveSignal.Set();
                //channel.BasicAck(ea.DeliveryTag, false);
            };
            // start consuming
            rabbitEvents.BasicConsume(context.Kafka.Events, false, consumer);
        }
    }

    public void ActiveMQ()
    {
        AutoResetEvent semaphore = new(false);
        ITextMessage? message = null;
        TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);
        // Example connection strings:
        //    activemq:tcp://activemqhost:61616
        //    stomp:tcp://activemqhost:61613
        //    ems:tcp://tibcohost:7222
        //    msmq://localhost

        Uri connecturi = new("activemq:tcp://localhost:61616");

        Console.WriteLine("About to connect to " + connecturi);

        // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
        var factory = new NMSConnectionFactory(connecturi);

        void OnMessage(Apache.NMS.IMessage receivedMsg)
        {
            message = receivedMsg as ITextMessage;
            semaphore.Set();
        }


        using Apache.NMS.IConnection connection = factory.CreateConnection("artemis", "artemis");

        using ISession session = connection.CreateSession();
        {
            IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");


            // Create a consumer and producer
            using IMessageConsumer consumer = session.CreateConsumer(destination);
            using IMessageProducer producer = session.CreateProducer(destination);
            // Start the connection so that messages will be processed.
            connection.Start();
            //producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
            producer.RequestTimeout = receiveTimeout;

            consumer.Listener += new MessageListener(OnMessage);

            // Send a message
            ITextMessage request = session.CreateTextMessage("Hello World!");
            request.NMSCorrelationID = "abc";
            request.Properties["NMSXGroupID"] = "cheese";
            request.Properties["myHeader"] = "Cheddar";

            producer.Send(request);

            // Wait for the message
            semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);
        }
    }

    static RabbitMQ.Client.IConnection GetConnection(string user, string pass, string vhost, string hostName)
    {
        ConnectionFactory factory = new() { Uri = new($@"amqp://{user}:{pass}@{hostName}:5672/") };

        return factory.CreateConnection();
    }

    //[Benchmark]
    public void RabbitMQ()
    {
        var grpc = Genie.Grpc.PartyRequest.Parser.ParseFrom(requestFile);
        grpc.Request.CosmosBase.Identifier.Id = Guid.NewGuid().ToString("N");

        var bytes = protobufSerializer.SerializeAsync(grpc, new SerializationContext()).GetAwaiter().GetResult();

        rabbitIngress?.ExchangeDeclare(this.context.RabbitMQ.Exchange, ExchangeType.Direct);
        rabbitIngress?.QueueDeclare(this.context.RabbitMQ.Queue, false, false, false, null);
        rabbitIngress?.QueueBind(this.context.RabbitMQ.Queue, this.context.RabbitMQ.Exchange, this.context.RabbitMQ.RoutingKey, null);

        rabbitIngress?.BasicPublish(context.RabbitMQ.Exchange, this.context.RabbitMQ.RoutingKey, null, bytes);

        // wait until message is received
        receiveSignal.WaitOne();
    }

    public void Inline()
    {
        var channel = rabbitIngress;

        //channel.ExchangeDeclare(this.context.Rabbit.Exchange, ExchangeType.Direct);
        //channel.QueueDeclare(this.context.Rabbit.Queue, false, false, false, null);
        //channel.QueueBind(this.context.Rabbit.Queue, this.context.Rabbit.Exchange, this.context.Rabbit.RoutingKey, null);

        var grpc = Genie.Grpc.PartyRequest.Parser.ParseFrom(requestFile);
        grpc.Request.CosmosBase.Identifier.Id = Guid.NewGuid().ToString("N");

        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Hello, world!");
        channel.BasicPublish(this.context.RabbitMQ.Exchange, this.context.RabbitMQ.RoutingKey, null, messageBodyBytes);

        AutoResetEvent hold = new(false);
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (ch, ea) =>
        {
            Console.WriteLine("Received");
            var body = ea.Body.ToArray();
            // copy or deserialise the payload
            // and process the message
            // ...
            //channel.BasicAck(ea.DeliveryTag, false);
            hold.Set();
        };
        // this consumer tag identifies the subscription
        // when it has to be cancelled
        string consumerTag = channel.BasicConsume(this.context.RabbitMQ.Queue, true, consumer);

        hold.WaitOne();

        _ = 0;
    }

    //[Benchmark]
    public void KafkaTopics()
    {
        // 10           | KafkaTopics | 321.6 ms | 6.37 ms | 9.53 ms |
        // 20           | KafkaTopics | 663.8 ms | 13.11 ms | 23.97 ms |
        // 40  (1)      | KafkaTopics | 1.121 s | 0.0114 s | 0.0107 s |
        // 40  (5)      | KafkaTopics | 1.064 s | 0.0086 s | 0.0080 s |
        // 40  (15)     | KafkaTopics | 1.063 s | 0.0054 s | 0.0048 s |
        // 40  (30)     | KafkaTopics | 1.063 s | 0.0042 s | 0.0039 s |
        // 40  (-1)     | KafkaTopics | 1.061 s | 0.0056 s | 0.0047 s |
        // 100 (30)     | KafkaTopics | 2.641 s | 0.0140 s | 0.0131 s |




        Parallel.For(0, 40, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
        {
            KafkaUtils.CreateTopic(adminClient, [Guid.NewGuid().ToString("N")]).GetAwaiter().GetResult();
        });
    }

    //[Benchmark]
    public void TestConversion()
    {
        var grpc = Genie.Grpc.PartyRequest.Parser.ParseFrom(requestFile);
        Parallel.For(0, 40, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
        {
            var partyRequest = CosmosAdapter.ToCosmos(grpc);
        });
    }

    //[Benchmark]
    public void Kafka()
    {
        using CancellationTokenSource cts = new();
        var grpc = Genie.Grpc.PartyRequest.Parser.ParseFrom(requestFile);
        grpc.Request.CosmosBase.Identifier.Id = Guid.NewGuid().ToString("N");

        var partyRequest = CosmosAdapter.ToCosmos(grpc);

        KafkaUtils.CreateTopic(adminClient, [grpc.Request.CosmosBase.Identifier.Id]).GetAwaiter().GetResult();

        KafkaUtils.Post(kafkaProducer, context.Kafka.Ingress, partyRequest,
            cts.Token).GetAwaiter().GetResult();


        //var consumerBuilder = new ConsumerBuilder<string, byte[]>(KafkaUtils.GetConfig(context));

        ////consumerBuilder.SetAvroKeyDeserializer(schemaRegistry);
        ////consumerBuilder.SetAvroValueDeserializer(schemaRegistry);
        //using var consumer = consumerBuilder.Build();


        kafkaConsumer.Subscribe(partyRequest.Id);
        var cr = kafkaConsumer.Consume(cts.Token);
        var result = cr.Message.Value;

        //consumer.Subscribe(partyRequest.Id);
        //var cr = consumer.Consume(cts.Token);
        //var result = cr.Message.Value;
    }



    //[Benchmark]
    public void ProtoActor()
    {
        var grpc = Genie.Grpc.PartyRequest.Parser.ParseFrom(requestFile);
        grpc.Request.CosmosBase.Identifier.Id = Guid.NewGuid().ToString("N");

        //var bytes = protobufSerializer.SerializeAsync(grpc, 
        //    new SerializationContext()).GetAwaiter().GetResult();

        _ = ActorUtils.InitiateActor(actorSystem, new Genie.Actors.GrainRequest
        {
            Key = grpc.GetType().Name,
            Request = new StatusRequest { Topic = grpc.Request.CosmosBase.Identifier.Id },
            Value = Any.Pack(grpc), // ByteString.CopyFrom(bytes),
            Timestamp = DateTime.UtcNow.ToTimestamp()
        }, true, new CancellationToken()).GetAwaiter().GetResult();
    }

    [Benchmark]
    public void PulsarNet()
    {
        Parallel.For(0, 10, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
        {
            var grpc = Genie.Grpc.PartyRequest.Parser.ParseFrom(requestFile);
            grpc.Request.CosmosBase.Identifier.Id = Guid.NewGuid().ToString("N");

            var bytes = protobufSerializer.SerializeAsync(grpc,
                new SerializationContext()).GetAwaiter().GetResult();

            pulsarProducer?.SendAsync(pulsarProducer.NewMessage(bytes, key: grpc.GetType().Name))
                .GetAwaiter().GetResult();

            var consumer = pulsarClient?
                .NewConsumer()
                //.NewConsumer(Pulsar.Client.Api.Schema.AVRO<EventTaskJob>())
                .Topic(grpc.Request.CosmosBase.Identifier.Id)
                .SubscriptionName("subscriptionName")
                .SubscribeAsync().GetAwaiter().GetResult();

            var message = consumer?.ReceiveAsync().GetAwaiter().GetResult();
            var resultData = message?.GetValue();

            consumer?.AcknowledgeAsync(message?.MessageId).GetAwaiter().GetResult();
        });


    }

    //[Benchmark]
    public void ApachePulsar()
    {
        var grpc = Genie.Grpc.PartyRequest.Parser.ParseFrom(requestFile);
        grpc.Request.CosmosBase.Identifier.Id = Guid.NewGuid().ToString("N");

        var bytes = protobufSerializer.SerializeAsync(grpc, 
            new SerializationContext()).GetAwaiter().GetResult();

        dotPulsarProducer?.NewMessage().Key(grpc.GetType().Name).Send(bytes, new CancellationToken()).GetAwaiter().GetResult();

        var consumer = dotPulsarClient?.NewConsumer(DotPulsar.Schema.ByteArray)
            .Topic(grpc.Request.CosmosBase.Identifier.Id)
            .SubscriptionName("subscriptionName")
            .Create();

        var message = consumer?.Receive().GetAwaiter().GetResult();

        var reader = new Chr.Avro.Serialization.BinaryReader(message?.Value());
        _ = binaryDeserializer(ref reader);

        consumer?.Acknowledge(message?.MessageId!).GetAwaiter().GetResult();
    }

}
