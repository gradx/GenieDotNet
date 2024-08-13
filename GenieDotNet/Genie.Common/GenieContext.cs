using Avro.Generic;
using Azure.Storage.Blobs;
using Confluent.Kafka;
using Genie.Common.Settings;
using MaxMind.Db;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace Genie.Common
{
    public class GenieContext
    {
        public AzureSettings Azure { get; set; }
        public KafkaSettings Kafka { get; set; }
        public RabbitSettings Rabbit { get; set; }

        public ActorSettings Actor { get; set; }
        public CosmosClient CosmosClient { get; set; }
        //public BlobServiceClient BlobServiceClient { get; set; }


        public GenieContext(IConfigurationRoot Configuration)
        {
            Azure = new AzureSettings(new AzureStorage(Environment.GetEnvironmentVariable("SECRET_STORAGE")!,
                        Configuration["Azure:Storage:server"]!,
                        Configuration["Azure:Storage:share"]!
                    ),
                     new AzureCosmos(Configuration["Azure:CosmosDB:id"]!, Configuration["Azure:CosmosDB:uri"]!,
                     Configuration["Azure:CosmosDB:key"] ?? Environment.GetEnvironmentVariable("SECRET_COSMOS")!));

            Kafka = new KafkaSettings(Configuration["Kafka:connectionString"]!, Configuration["Kafka:ingress"]!,
                Configuration["Kafka:events"]!, Configuration["Kafka:mountPath"]!);

            Actor = new ActorSettings(Configuration["Actor:clusterName"]!, Configuration["Actor:consulProvider"]!);

            Rabbit = new RabbitSettings(Configuration["Rabbit:exchange"]!, Configuration["Rabbit:queue"]!, Configuration["Rabbit:routingKey"]!,
                Configuration["Rabbit:user"]!, Configuration["Rabbit:pass"]!, Configuration["Rabbit:vhost"]!, Configuration["Rabbit:host"]!);

            CosmosClient = new CosmosClientBuilder(Azure.CosmosDB.Uri, Azure.CosmosDB.Key)
                .WithCustomSerializer(new GeoJsonCosmosSerializer())
                .Build();

            
            //if (!string.IsNullOrEmpty(Azure.Storage.ConnectionString))
            //    BlobServiceClient = new BlobServiceClient(Azure.Storage.ConnectionString);
        }

        public static (GenieContext GenieContext, HostApplicationBuilder Host) Build()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var host = Host.CreateApplicationBuilder();
            host.Configuration.AddConfiguration(configuration);

            return new(new GenieContext(host.Configuration), host);
        }
    }
}
