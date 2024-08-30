using System.Text.Json.Serialization;


namespace Genie.Common.Types;

[Serializable]
public record CosmosFeatureCollection : NetTopologyFeatureCollection
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "";

    public string _rid { get; set; } = "";

    public string _self { get; set; } = "";

    public string _etag { get; set; } = "";

    public string _attachments { get; set; } = "";

    public int? _ts { get; set; }

    public int? ttl { get; set; }
}


