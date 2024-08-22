using Apache.Arrow;
using GeoParquet;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using NpgsqlTypes;
using ParquetSharp;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Scratch
{
    public class LoadPostgis
    {
        public static async Task AddIndex()
        {
            var connectionString = "Host=localhost;Username=postgres;Password=genie_in_a_bottle;Database=postgres";
            var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dsBuilder.UseNetTopologySuite();
            await using var dataSource = dsBuilder.Build();

            await using var command = dataSource.CreateCommand("CREATE INDEX idx_geoshape ON zcta5 USING gist (geoshape);");
            await command.ExecuteNonQueryAsync();
        }
        public static async Task Start()
        {

            var connectionString = "Host=localhost;Username=postgres;Password=genie_in_a_bottle;Database=postgres";
            var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dsBuilder.UseNetTopologySuite();
            await using var dataSource = dsBuilder.Build();

            await using var command = dataSource.CreateCommand("DROP DATABASE genie;");
            await command.ExecuteNonQueryAsync();

            command.CommandText = "CREATE DATABASE genie;";
            await command.ExecuteNonQueryAsync();

            command.CommandText = "DROP TABLE zcta5;";
            await command.ExecuteNonQueryAsync();

            command.CommandText = @"CREATE TABLE zcta5 (
                  zcta5_code VARCHAR (255) PRIMARY KEY, 
                  zcta5_name VARCHAR (255),
                  geoshape GEOMETRY(MULTIPOLYGON,4326) NOT NULL, 
                  ste_code VARCHAR (255),
                  ste_name VARCHAR (255),
                  coty_code VARCHAR (255),
                  coty_name VARCHAR (255),
                  zcta5_area_code VARCHAR (255),
                  zcta5_type VARCHAR (255),
                  zcta5_name_long INTEGER
                );";
            await command.ExecuteNonQueryAsync();

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

                await using (var cmd = dataSource.CreateCommand(@"INSERT INTO 
                    zcta5(zcta5_code,zcta5_name,geoshape,ste_code,ste_name,coty_code,coty_name,zcta5_area_code,zcta5_type,zcta5_name_long) 
                    VALUES($1,$2,ST_Multi($3),$4,$5,$6,$7,$8,$9,$10)"))
                {
                    cmd.Parameters.Add(new() { Value = zcta5_code[i] });
                    cmd.Parameters.Add(new() { Value = zcta5_name[i] });
                    cmd.Parameters.Add(new() { Value = geo, NpgsqlDbType = NpgsqlDbType.Geometry });
                    cmd.Parameters.Add(new() { Value = ste_code[i] });
                    cmd.Parameters.Add(new() { Value = ste_name[i] });
                    cmd.Parameters.Add(new() { Value = coty_code[i] });
                    cmd.Parameters.Add(new() { Value = coty_name[i] });
                    cmd.Parameters.Add(new() { Value = zcta5_area_code[i] });
                    cmd.Parameters.Add(new() { Value = zcta5_type[i] });
                    cmd.Parameters.Add(new() { Value = zcta5_name_long[i] == null ? DBNull.Value : zcta5_name_long[i], NpgsqlDbType = NpgsqlDbType.Integer });
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }
    }
}
