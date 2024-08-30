using Geodesy;
using NetTopologySuite.Geometries;

namespace Genie.Common.Utils;

public class GeometryCalculator
{
    // latitude = Y
    // longitude = X

    public enum Shape
    {
        Square = 4,
        Circle = 24
    }

    public static double Distance(Geometry start, Geometry end)
    {
        GeodeticCalculator geoCalc = new(Ellipsoid.WGS84);

        var result = geoCalc.CalculateGeodeticMeasurement(new GlobalPosition(new GlobalCoordinates(new Angle(start.Centroid.Y), new Angle(start.Centroid.X))),
            new GlobalPosition(new GlobalCoordinates(new Angle(end.Centroid.Y), new Angle(end.Centroid.X))));

        return result.PointToPointDistance;
    }


    public static Geometry Polygon(double lat, double lon, double meters, GeometryCalculator.Shape sides)
    {
        return Polygon(lat, lon, meters, (int)sides);
    }

    public static Geometry Polygon(double lat, double lon, double meters, int sides)
    {
        GeodeticCalculator geoCalc = new(Ellipsoid.WGS84);
        GlobalCoordinates start = new(new Angle(lat), new Angle(lon));
        int degree_unit = 360 / sides;

        var c = new List<Coordinate>();

        for (int i = 0; i < sides; i++)
        {
            GlobalCoordinates dest = geoCalc.CalculateEndingGlobalCoordinates(start, new Angle(degree_unit * i), meters);
            c.Add(new Coordinate(dest.Longitude.Degrees, dest.Latitude.Degrees));
        }
        c.Add(c[0]);
        c.Reverse();

        GeometryFactory gc = new(new PrecisionModel(), 4326);
        var box = gc.CreatePolygon([..c]);

        return box;
    }
}