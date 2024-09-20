using Chr.Avro.Confluent;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Genie.Actors;
using Genie.Adapters.Brokers.ActiveMQ;
using Genie.Adapters.Brokers.Aeron;
using Genie.Adapters.Brokers.Kafka;
using Genie.Adapters.Brokers.MQTT;
using Genie.Adapters.Brokers.NATS;
using Genie.Adapters.Brokers.Pulsar;
using Genie.Adapters.Brokers.RabbitMQ;
using Genie.Adapters.Brokers.ZeroMQ;
using Genie.Adapters.Persistence.Aerospike;
using Genie.Adapters.Persistence.ArangoDB;
using Genie.Adapters.Persistence.Cassandra;
using Genie.Adapters.Persistence.CockroachDB;
using Genie.Adapters.Persistence.Couchbase;
using Genie.Adapters.Persistence.CouchDB;
using Genie.Adapters.Persistence.CrateDB;
using Genie.Adapters.Persistence.Elasticsearch;
using Genie.Adapters.Persistence.MariaDB;
using Genie.Adapters.Persistence.Marten;
using Genie.Adapters.Persistence.Milvus;
using Genie.Adapters.Persistence.MongoDB;
using Genie.Adapters.Persistence.Neo4j;
using Genie.Adapters.Persistence.RavenDB;
using Genie.Adapters.Persistence.Redis;
using Genie.Common;
using Genie.Common.Performance;
using Genie.Common.Utils;
using Genie.Extensions.Genius;
using Genie.Web.Api.Actor;
using Genie.Web.Api.Rest;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using ZLogger;
using ZLogger.Providers;


var app = Build(args);
Configure(app);
app.Run();


static WebApplication Build(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddGrpc();


    var genieContext = GenieContext.Build().GenieContext;
    builder.Services.AddSingleton(genieContext);

    // Shared
    var config = AvroSupport.GetSchemaRegistryConfig();
    var schemaRegistry = new CachedSchemaRegistryClient(config);
    var schemaBuilder = AvroSupport.GetSchemaBuilder();

    builder.Services.AddSingleton(schemaRegistry);
    builder.Services.AddSingleton(schemaBuilder);

    // Object Pools
    builder.Services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

    builder.Services.TryAddSingleton(serviceProvider =>
    {
        var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
        var policy = new DefaultPooledObjectPolicy<GeniePooledObject>();
        //var policy = new LimitedPooledObjectPolicy<GeniePooledObject>(8);
        return provider.Create(policy);
    });

    ConfigureMessageQueuePools(builder);
    ConfigureDatabasePools(builder);

    // Kafka
    var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = genieContext.Kafka.Host }).Build();
    builder.Services.AddSingleton(adminClient);

    // Avro
    var producerBuilder = new ProducerBuilder<string, Genie.Common.Types.PartyRequest>(new ProducerConfig
    {
        BootstrapServers = genieContext.Kafka.Host
    });
    producerBuilder.SetAvroKeySerializer(schemaRegistry,
        $"{typeof(Genie.Common.Types.PartyRequest).FullName}-Key", registerAutomatically: AutomaticRegistrationBehavior.Always).GetAwaiter().GetResult(); ;
    producerBuilder.SetAvroValueSerializer(AvroSupport.GetSerializerBuilder(config, schemaBuilder),
        $"{typeof(Genie.Common.Types.PartyRequest).FullName}-Value", registerAutomatically: AutomaticRegistrationBehavior.Always).GetAwaiter().GetResult();
    IProducer<string, Genie.Common.Types.PartyRequest> producer = producerBuilder.Build();

    builder.Services.AddSingleton(producer);

    // Actor
    builder.Services.AddSingleton(provider =>
    {
        return ActorUtils.JoinActorSystem(genieContext.Actor.ClusterName, genieContext.Actor.ConsulProvider, [GeniusGrainReflection.Descriptor, ActorGrainReflection.Descriptor]);
    });

    builder.Services.AddHostedService<ActorSystemClusterHostedService>();


    // Mediator
    builder.Services.AddMediator(options =>
    {
        options.Namespace = "Genie.Mediator";
        options.ServiceLifetime = ServiceLifetime.Transient;
    });


    builder.Logging
        .AddZLoggerRollingFile(options =>
        {

            // File name determined by parameters to be rotated
            options.FilePathSelector = (timestamp, sequenceNumber) => $"c:\\temp\\web\\{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";

            // The period of time for which you want to rotate files at time intervals.
            options.RollingInterval = RollingInterval.Day;

            // Limit of size if you want to rotate by file size. (KB)
            options.RollingSizeKB = 1024;
        })
        .AddZLoggerConsole(options =>
        {
            options.UseJsonFormatter();
        })
        // Format as json and configure output
        .AddZLoggerConsole(options =>
        {
            options.UseJsonFormatter(formatter =>
            {
                formatter.IncludeProperties = IncludeProperties.ParameterKeyValues;
            });
        })
        // Further common settings
        .AddZLoggerConsole(options =>
        {
            // Enable LoggerExtensions.BeginScope
            options.IncludeScopes = true;
        });

    return builder.Build();

    static void ConfigureDatabasePools(WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new LimitedPooledObjectPolicy<AerospikePooledObject>(100);
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<ArangoPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<CassandraPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<CockroackPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<CouchbasePooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<CouchPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<CratePooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<ElasticsearchPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<MariaPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<MartenPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<MilvusPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<MongoPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<Neo4jPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<RavenPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<RedisPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<ScyllaPooledObject>();
            return provider.Create(policy);
        });
    }

    static void ConfigureMessageQueuePools(WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<PulsarPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<RabbitMQPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<ActiveMQPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<KafkaPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<ZeroMQPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<AeronPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<AeronServicePooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<NatsPooledObject>();
            return provider.Create(policy);
        });

        builder.Services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<MQTTPooledObject>();
            return provider.Create(policy);
        });
    }
}

static void Configure(WebApplication app)
{
    PartyEndpoints.Map(app);

    app.UseRouting().UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<GeniusEventRPCService>();
        endpoints.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
    });
}
