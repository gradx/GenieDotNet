
using System.Text.Json.Serialization;

namespace Genie.Common.Types;
public record GeoJsonShape
{
    [JsonPropertyName("centroid")]
    public Coordinate? Centroid { get; set; }
}