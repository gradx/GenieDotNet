

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using Genie.Common.Performance;
using Genie.Common.Utils;
using GeoParquet;
using ParquetSharp;
using System.Diagnostics;

namespace Genie.Scratch
{


    public class LoadElastic
    {
        private const string c_Index = "testindex3";

        public async static Task Test()
        {
            // 
            var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
                .Authentication(new BasicAuthentication("elastic", "7PMxQyEb+=aNDV2fVlw="))
                .CertificateFingerprint("d96300dd6d8a14c4df76f57127362d772380e3fdcbfe0a72505a1cdc240fbdb7");

            settings.DisableDirectStreaming(true);
            settings.DefaultIndex(c_Index);

            var client = new ElasticsearchClient(settings);


            var geo = GeometryCalculator.Polygon(38.89781822004474, -77.03655126065402, 10, 4);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var respose = await client.SearchAsync<ElasticGeo>(s => s.Index(c_Index).From(0).Size(10).Query(q =>
            {
                //q.GeoBoundingBox(g => g.Field(p => p.testshape).BoundingBox(new TopLeftBottomRightGeoBounds
                //{
                //    TopLeft = new LatLonGeoLocation { Lat = 144.334097, Lon = 2.494068 },
                //    BottomRight = new LatLonGeoLocation { Lat = -43.279707, Lon = 49.506155 },
                //})
                //.ValidationMethod(Elastic.Clients.Elasticsearch.QueryDsl.GeoValidationMethod.Strict).IgnoreUnmapped(true));


                //q.GeoShape(v => v.Field(xs => xs.testshape).Shape(bb => bb.Relation(GeoShapeRelation.Intersects).Shape(geo)));
                //q.Shape(l => l.Field(x => x.GeoShape).Shape(bb => bb.Relation(GeoShapeRelation.Intersects)));
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
                //q.Match(r => r.Field(f => f.GeoShape));
                //q.Match(r => r.Field(f => f.CountyName).Query("Philadelphia"));
            }));

            stopwatch.Stop();


        }
        public async static Task Start()
        {
            // 
            var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
                .Authentication(new BasicAuthentication("elastic", "7PMxQyEb+=aNDV2fVlw="))
                .CertificateFingerprint("d96300dd6d8a14c4df76f57127362d772380e3fdcbfe0a72505a1cdc240fbdb7");

            settings.DisableDirectStreaming(true);
            settings.DefaultIndex(c_Index);
            var client = new ElasticsearchClient(settings);

            var result = await client.Cluster.HealthAsync();


            var indexReq = new Elastic.Clients.Elasticsearch.IndexManagement.CreateIndexRequest(c_Index);
            indexReq.Mappings = new TypeMapping();
            indexReq.Mappings.Properties = new Properties();
            indexReq!.Mappings!.Properties!.Add(new PropertyName("testshape"), new GeoShapeProperty());
            await client.Indices.CreateAsync(indexReq);

            var file1 = new ParquetFileReader(@"c:\temp\georef-united-states-of-america-zcta5-cleansed.parquet");
            var geoParquet = file1.GetGeoMetadata();

            var rowGroupReader = file1.RowGroup(0);

            var geoshape = rowGroupReader.Column(0).LogicalReader<byte[]>().ToList();
            var ste_code = rowGroupReader.Column(2).LogicalReader<string>().ToList();
            var ste_name = rowGroupReader.Column(3).LogicalReader<string>().ToList();
            var coty_code = rowGroupReader.Column(4).LogicalReader<string>().ToList();
            var coty_name = rowGroupReader.Column(5).LogicalReader<string>().ToList();
            var zcta5_code = rowGroupReader.Column(6).LogicalReader<string>().ToList();
            var zcta5_name = rowGroupReader.Column(7).LogicalReader<string>().ToList();
            var zcta5_area_code = rowGroupReader.Column(8).LogicalReader<string>().ToList();
            var zcta5_type = rowGroupReader.Column(9).LogicalReader<string>().ToList();
            var zcta5_name_long = rowGroupReader.Column(10).LogicalReader<int?>().ToList();

            NetTopologySuite.IO.WKBReader r = new NetTopologySuite.IO.WKBReader();
            for (int i = 0; i < file1.FileMetaData.NumRows; i++)
            {
                Console.WriteLine($@"Inserting {i} of {file1.FileMetaData.NumRows}");
                var geo = r.Read(geoshape[i]);
                var elasticGeo = new ElasticGeo {
                    testshape = geo.ToText(),
                    StateCode = ste_code[i],
                    StateName = ste_name[i],
                    CountyCode = coty_code[i],
                    CountyName = coty_code[i],
                    Zcta5Code = zcta5_code[i],
                    Zcta5Name = zcta5_name[i],
                    Zcta5AreaCode = zcta5_area_code[i],
                    Zcta5Type = zcta5_type[i],
                    Zcta5NameLong = zcta5_name_long[i],
                };
                var response = await client.IndexAsync(elasticGeo);
            }
        }
    }
}
