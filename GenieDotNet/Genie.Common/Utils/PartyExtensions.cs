using Genie.Common.Adapters;
using Genie.Common.Types;
using NetTopologySuite.Features;

namespace Genie.Common.Utils;

public static class PartyExtensions
{
    public static void ReversGeoCode(this Party p)
    {
        p.Communications.ForEach(e =>
        {
            foreach (var a in e.CommunicationIdentity!.GeographicLocation!.GeoJsonLocation!.Features)
            {
                a.Attributes = OvertureMapAdapter.ReverseGeoCode(a.Geometry, (AttributesTable)a.Attributes);
            }
        });
    }
}