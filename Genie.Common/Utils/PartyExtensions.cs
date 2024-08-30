using Genie.Common.Adapters;
using Genie.Common.Types;
using Genie.Common.Web;
using Microsoft.Extensions.ObjectPool;
using NetTopologySuite.Features;

namespace Genie.Common.Utils;

public static class PartyExtensions
{
    public static void ReversGeoCode<T>(this Party p, ObjectPool<T> pool) where T : class
    {
        p.Communications.ForEach(e =>
        {
            foreach (var a in e.CommunicationIdentity!.GeographicLocation!.GeoJsonLocation!.Features)
            {
                a.Attributes = MapAdapter.ReverseGeoCode(pool, a.Geometry, (AttributesTable)a.Attributes);
            }
        });
    }
}