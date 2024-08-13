using System.Text.Json.Serialization;

namespace Genie.Common.Types;

public record GeoJsonLocation : CosmosFeatureCollection
{
    [JsonPropertyName("shape")]
    public GeoJsonShape? Shape { get; set; }
    [JsonPropertyName("schedules")]
    public List<Schedule> Schedules { get; set; } = [];
    [JsonPropertyName("assessments")]
    public List<Assessment> Assessments { get; set; } = [];
}