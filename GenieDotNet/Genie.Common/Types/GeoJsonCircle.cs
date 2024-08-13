
using System.Text.Json.Serialization;

namespace Genie.Common.Types;
public record GeoJsonCircle : GeoJsonShape
{
    [JsonPropertyName("radius")]
    public int Radius { get; set; }
}