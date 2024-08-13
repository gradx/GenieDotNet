namespace Genie.Common.Types;

public record GeographicLocation
{
    public string? GeographicLocationTypeCode { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationName { get; set; }
    public string? LocationNumber { get; set; }
    public string? StateCode { get; set; }
    public LocationAddress? LocationAddress { get; set; }
    public GeoJsonLocation? GeoJsonLocation { get; set; }
}