using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Genie.Adapters.Persistence.Elasticsearch;

public class ElasticsearchPooledObject
{
    public ElasticsearchClient Client { get; init; }

    public ElasticsearchPooledObject()
    {
        var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
            .CertificateFingerprint("b398f613001f05bda960c6e7f8c3e5c20ffeb725a2c3cb1d7e83628ba1ed86ed")
            .Authentication(new BasicAuthentication("elastic", "VEbtt3GW_2WcXyw47Nl6"));
        settings.DisableDirectStreaming(true);
        //settings.DefaultIndex("genie");

        Client = new ElasticsearchClient(settings);
    }
}