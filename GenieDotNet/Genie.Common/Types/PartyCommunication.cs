using System;

namespace Genie.Common.Types;
public record PartyCommunication
{
    public DateTime BeginDate { get; set; }
    public string? LocalityCode { get; set; }
    public CommunicationIdentity? CommunicationIdentity { get; set; }
    public DateTime? EndDate { get; set; }

    public bool HasSpatialFeatures()
    {
        return (this.CommunicationIdentity != null && this.CommunicationIdentity.GeographicLocation != null
            && this.CommunicationIdentity.GeographicLocation.GeoJsonLocation != null && this.CommunicationIdentity.GeographicLocation.GeoJsonLocation.Features.Count > 0);
    }
}