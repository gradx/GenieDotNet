using Chr.Avro.Abstract;
using Chr.Avro.Confluent;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Confluent.SchemaRegistry;
using Genie.Common.Utils;

namespace Genie.Common.Adapters.Kafka;

public class KafkaUtils
{
    public static async Task Post<T>(SchemaBuilder schemaBuilder, CachedSchemaRegistryClient schemaRegistry, string host, string topic, T request, CancellationToken cancellationToken)
    {
        var registryConfig = AvroSupport.GetSchemaRegistryConfig();

        var producerBuilder = new ProducerBuilder<string, T>(new ProducerConfig { BootstrapServers = host });
        await Task.WhenAll(
            producerBuilder.SetAvroKeySerializer(schemaRegistry,
                $"{typeof(T).FullName}-Key", registerAutomatically: AutomaticRegistrationBehavior.Always),
            producerBuilder.SetAvroValueSerializer(AvroSupport.GetSerializerBuilder(registryConfig, schemaBuilder),
                $"{typeof(T).FullName}-Value", registerAutomatically: AutomaticRegistrationBehavior.Always)
        );

        //Use name as shortcut
        using var producer = producerBuilder.Build();
        await producer.ProduceAsync(topic, new Message<string, T> { Key = typeof(T).Name!, Value = request }, cancellationToken);
    }

    public static async Task Post<T>(IProducer<string, T> producer, string topic, T request, CancellationToken cancellationToken)
    {

        await producer.ProduceAsync(topic, new Message<string, T> { Key = typeof(T).Name!, Value = request }, cancellationToken);
    }


    public static async Task<bool> CreateTopic(IAdminClient adminClient, string[] topic)
    {
        bool success = false;

        try
        {
            await adminClient.CreateTopicsAsync(topic.Select(t => new TopicSpecification
            {
                Name = t,
                NumPartitions = 1
            }));

            success = true;
        }
        catch (Exception ex) when (ex is CreateTopicsException && ex.Message.Contains("already exists"))
        {

        }

        return success;
    }

    public static async Task DeleteTopic(string host, string topic)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = host }).Build();
        await adminClient.DeleteTopicsAsync([topic]);
    }

    public static List<TopicMetadata> GetTopics(string host)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = host }).Build();
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        return metadata.Topics.ToList();
    }
    public static ConsumerConfig GetConfig(GenieContext context)
    {
        return new ConsumerConfig
        {
            GroupId = Guid.NewGuid().ToString(),
            BootstrapServers = context.Kafka.Host,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SessionTimeoutMs = 10000,
            SocketTimeoutMs = 10000,
            MaxPollIntervalMs = 10000,
        };
    }


}