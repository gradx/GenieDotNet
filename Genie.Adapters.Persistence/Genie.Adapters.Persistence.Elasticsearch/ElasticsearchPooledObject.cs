using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Genie.Adapters.Persistence.Elasticsearch;

public class ElasticsearchPooledObject
{
    public ElasticsearchClient Client { get; init; }

    public ElasticsearchPooledObject()
    {
        var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
            .CertificateFingerprint("e112803ddc79f4d09d3845d19e7071a14e1dc5c5c8a2619e29b7d98aed880df7")
            .Authentication(new BasicAuthentication("elastic", "j*Ro9gRT95I_kfewSmrW"));
        settings.DisableDirectStreaming(true);
        settings.DefaultIndex("genie");

        Client = new ElasticsearchClient(settings);
    }
}