using Genie.Common.Types;
using Genie.Common.Utils;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;


namespace Genie.Common.Adapters
{
    public class OvertureMapAdapter
    {
        public static void ReverseGeoCode(Party p)
        {
            p.Communications.ForEach(e =>
            {
                foreach (var a in e.CommunicationIdentity!.GeographicLocation!.GeoJsonLocation!.Features)
                {
                    a.Attributes = ReverseGeoCode(a.Geometry, (AttributesTable)a.Attributes);
                }
            });
        }

        public static AttributesTable ReverseGeoCode(Geometry geo, AttributesTable attrs)
        {
            Mutex mapMutex = new Mutex(false, "OvertureMaps");
            mapMutex.WaitOne();
            var mapConn = DuckDbSupport.InstanceSpatial;
            var command = mapConn.CreateCommand();

            command.CommandText = $@"SELECT * FROM world WHERE 
            ST_AREA(ST_Intersection(geo_shape, ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                attrs.Add("name", reader.GetString(reader.GetOrdinal("name")));
                attrs.Add("iso3", reader.GetString(reader.GetOrdinal("iso3")));
                attrs.Add("continent", reader.GetString(reader.GetOrdinal("continent")));
                attrs.Add("region", reader.GetString(reader.GetOrdinal("region")));
            }

            command.CommandText = $@"SELECT * FROM state WHERE ST_AREA(ST_Intersection(geo_shape, 
            ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                attrs.Add("ste_code", reader.GetString(reader.GetOrdinal("ste_code")));
                attrs.Add("ste_name", reader.GetString(reader.GetOrdinal("ste_name")));
            }

            command.CommandText = $@"SELECT * FROM county WHERE 
            ST_AREA(ST_Intersection(geo_shape, ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                attrs.Add("coty_code", reader.GetString(reader.GetOrdinal("coty_code")));
                attrs.Add("coty_name", reader.GetString(reader.GetOrdinal("coty_name")));
                attrs.Add("coty_name_long", reader.GetString(reader.GetOrdinal("coty_name_long")));
            }

            command.CommandText = $@"SELECT * FROM zcta WHERE 
            ST_AREA(ST_Intersection(geo_shape, ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                attrs.Add("zcta5_code", reader.GetString(reader.GetOrdinal("zcta5_code")));
                attrs.Add("zcta5_name", reader.GetString(reader.GetOrdinal("zcta5_name")));
            }

            command.CommandText = $@"SELECT * FROM place WHERE 
            ST_AREA(ST_Intersection(geo_shape, ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                attrs.Add("pla_code", reader.GetString(reader.GetOrdinal("pla_code")));
                attrs.Add("pla_name", reader.GetString(reader.GetOrdinal("pla_name")));
                attrs.Add("pla_name_long", reader.GetString(reader.GetOrdinal("pla_name_long")));
            }


            mapMutex.ReleaseMutex();

            return attrs;
        }


    }
}
