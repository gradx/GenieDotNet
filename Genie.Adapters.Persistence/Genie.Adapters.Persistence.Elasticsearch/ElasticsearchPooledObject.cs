using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Genie.Adapters.Persistence.Elasticsearch;

public class ElasticsearchPooledObject
{
    public ElasticsearchClient Client { get; init; }

    public ElasticsearchPooledObject()
    {
        var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
            .CertificateFingerprint("1b1aac60cfc6d4a16c3942048fd24819f67db3389aa380acdacdd21629eb19e9")
            .Authentication(new BasicAuthentication("elastic", "hUSX9dRG-499_uR-+LV+"));
        settings.DisableDirectStreaming(true);
        settings.DefaultIndex("genie");

        Client = new ElasticsearchClient(settings);
    }
}