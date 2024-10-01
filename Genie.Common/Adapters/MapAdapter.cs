using Elastic.Clients.Elasticsearch;
using Genie.Adapters.Persistence.Elasticsearch;
using Genie.Adapters.Persistence.Postgres;
using Genie.Common.Performance;
using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.IO.Hashing;


namespace Genie.Common.Adapters
{

    public class MapAdapter
    {
        private readonly static XxHash64 hasher = new();

        public static AttributesTable ReverseGeoCode<T>(ObjectPool<T> pool, Geometry geo, AttributesTable attrs) where T : class
        {
            var pooled = pool.Get();

            if(pooled is ElasticsearchPooledObject e)
            {
                
                var respose = e.Client.SearchAsync<ElasticGeo>(s => s.Index("elastic_geo").From(0).Size(10).Query(q =>
                {
                    q.GeoShape(new Elastic.Clients.Elasticsearch.QueryDsl.GeoShapeQuery
                    {
                        Field = new Field("testshape"),
                        IgnoreUnmapped = true,
                        Shape = new Elastic.Clients.Elasticsearch.QueryDsl.GeoShapeFieldQuery
                        {
                            Shape = geo.ToText(),
                            Relation = GeoShapeRelation.Intersects,
                        },
                    });
                }));

                var doc = respose.Result.Documents.FirstOrDefault();
                if (doc != null)
                {
                    attrs.Add("zcta5_code", doc.Zcta5Code);
                    attrs.Add("zcta5_name", doc.Zcta5Name);
                }

            }
            else if (pooled is PostgresPooledObject p)
            {
                var geoquery = @"SELECT * FROM zcta5 WHERE ST_Intersects(geoshape,$1)";
                using var cmd = p.DataSource.CreateCommand(geoquery);
                cmd.Parameters.Add(new() { Value = geo });
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    attrs.Add("zcta5_code", reader.GetString(reader.GetOrdinal("zcta5_code")));
                    attrs.Add("zcta5_name", reader.GetString(reader.GetOrdinal("zcta5_name")));
                }

            }
            else if (pooled is DuckDbPooledObject d)
            {
                //Mutex mapMutex = new Mutex(false, "OvertureMaps");
                //mapMutex.WaitOne();
                //var mapConn = DuckDbSupport.InstanceSpatial;

                hasher.Append(geo.AsBinary());


                //ObjectCache cache = MemoryCache.Default;
                //var cached = cache[hashKey];
                //if (cached != null)
                //    return (AttributesTable)cached;

                var geojson = GeoJsonCosmosSerializer.ToJson(geo);

                var mapConn = d.Connection;
                var command = mapConn.CreateCommand();

                //command.CommandText = $@"SELECT * FROM world WHERE 
                //ST_AREA(ST_Intersection(geo_shape, ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
                //var reader = command.ExecuteReader();

                //while (reader.Read())
                //{
                //    attrs.Add("name", reader.GetString(reader.GetOrdinal("name")));
                //    attrs.Add("iso3", reader.GetString(reader.GetOrdinal("iso3")));
                //    attrs.Add("continent", reader.GetString(reader.GetOrdinal("continent")));
                //    attrs.Add("region", reader.GetString(reader.GetOrdinal("region")));
                //}

                //command.CommandText = $@"SELECT * FROM state WHERE ST_AREA(ST_Intersection(geo_shape, 
                //ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
                //reader = command.ExecuteReader();
                //while (reader.Read())
                //{
                //    attrs.Add("ste_code", reader.GetString(reader.GetOrdinal("ste_code")));
                //    attrs.Add("ste_name", reader.GetString(reader.GetOrdinal("ste_name")));
                //}

                //command.CommandText = $@"SELECT * FROM county WHERE 
                //ST_AREA(ST_Intersection(geo_shape, ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
                //reader = command.ExecuteReader();
                //while (reader.Read())
                //{
                //    attrs.Add("coty_code", reader.GetString(reader.GetOrdinal("coty_code")));
                //    attrs.Add("coty_name", reader.GetString(reader.GetOrdinal("coty_name")));
                //    attrs.Add("coty_name_long", reader.GetString(reader.GetOrdinal("coty_name_long")));
                //}

                command.CommandText = $@"SELECT zcta5_code, zcta5_name FROM zcta WHERE 
                        ST_Intersects(geo_shape, ST_GeomFromGeoJSON('{geojson}')) > 0;";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    attrs.Add("zcta5_code", reader.GetString(reader.GetOrdinal("zcta5_code")));
                    attrs.Add("zcta5_name", reader.GetString(reader.GetOrdinal("zcta5_name")));
                }

                //command.CommandText = $@"SELECT * FROM place WHERE 
                //ST_AREA(ST_Intersection(geo_shape, ST_GeomFromGeoJSON('{GeoJsonCosmosSerializer.ToJson(geo)}'))) > 0;";
                //reader = command.ExecuteReader();
                //while (reader.Read())
                //{
                //    attrs.Add("pla_code", reader.GetString(reader.GetOrdinal("pla_code")));
                //    attrs.Add("pla_name", reader.GetString(reader.GetOrdinal("pla_name")));
                //    attrs.Add("pla_name_long", reader.GetString(reader.GetOrdinal("pla_name_long")));
                //}
            }



            pool.Return(pooled);

            return attrs;
        }
    }
}