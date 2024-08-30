using DeepEqual;
using DeepEqual.Syntax;
using Genie.Common.Types;

namespace Genie.Common.Utils;


public static class StringExtensions
{
    public static string? Null(this string s) => string.IsNullOrEmpty(s) ? null : s;

    public static string Empty(this string s) => string.IsNullOrEmpty(s) ? "" : s;

    public static string NullOrEmpty(this string s, string value) => string.IsNullOrEmpty(s) ? value : s;

    public static byte[] Hex(this string hex) => Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
}

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
        catch(DeepEqualException ex)
        {
            return ex.Context;
        }

        return null;
    }
}