namespace Genie.Common.Types;

public record CommunicationIdentity
{
    public enum CommunicationType
    {
        Email = 0,
        Phone = 1,
        Address = 2,
        Provider = 3,
        Geospatial = 4
    }
    public CommunicationType Relationship { get; set; }

    public string QualifierValue { get; set; } = "";
    public GeographicLocation? GeographicLocation { get; set; }
}