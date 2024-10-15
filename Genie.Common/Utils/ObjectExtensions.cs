using DeepEqual.Syntax;
using DeepEqual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genie.Common.Types;

namespace Genie.Common.Utils;

public static class ObjectExtensions
{
    public static IComparisonContext? GetDeepEqualComparison(this object o, object expected)
    {
        try
        {
            var x = new ComparisonBuilder();
            x.IgnoreCircularReferences();
            x.IgnoreProperty<GeoJsonLocation>(e => e.Features);

            o.ShouldDeepEqual(expected, x.Create());
        }
        catch (DeepEqualException ex)
        {
            return ex.Context;
        }

        return null;
    }
}