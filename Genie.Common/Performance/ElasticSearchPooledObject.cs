using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Genie.Common.Performance;

public class ElasticGeo
{
    public string Id { get; set; }
    public string testshape { get; set; }
    public string StateCode { get; set; }
    public string StateName { get; set; }
    public string CountyCode { get; set; }
    public string CountyName { get; set; }
    public string Zcta5Code { get; set; }
    public string Zcta5Name { get; set; }
    public string Zcta5Type { get; set; }
    public string Zcta5AreaCode { get; set; }
    public int? Zcta5NameLong { get; set; }

}

public class ElasticSearchPooledObject
{
    public ElasticsearchClient Client { get; init; }

    public ElasticSearchPooledObject()
    {
        var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
            .Authentication(new BasicAuthentication("elastic", "7PMxQyEb+=aNDV2fVlw="))
            .CertificateFingerprint("d96300dd6d8a14c4df76f57127362d772380e3fdcbfe0a72505a1cdc240fbdb7");

        settings.DisableDirectStreaming(true);
        settings.DefaultIndex("testindex3");

        Client = new ElasticsearchClient(settings);
    }
}


