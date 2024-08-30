using Apache.NMS.ActiveMQ.Commands;
using DuckDB.NET.Data;
using Genie.Common.Web;
using System.Data.Common;

namespace Genie.Common.Utils;


public class DuckDbSupport
{
    private static readonly Lazy<DuckDBConnection> lazySpatial = new(() => GetSpatialDb());
    private static readonly Lazy<DuckDBConnection> lazyOverture = new(() => GetOvertureDb());

    private DuckDbSupport() { }

    public static DuckDBConnection InstanceSpatial => lazySpatial.Value;
    public static DuckDBConnection InstanceOverture => lazyOverture.Value;

    public static DuckDBConnection GetSpatialDb()
    {
        string world = @"C:\temp\world-administrative-boundaries-cleansed.parquet";
        string state = @"C:\temp\georef-united-states-of-america-state-cleansed.parquet";
        string county = @"C:\temp\georef-united-states-of-america-county-cleansed.parquet";
        string zcta = @"C:\temp\georef-united-states-of-america-zcta5-cleansed.parquet";
        string place = @"C:\temp\georef-united-states-of-america-place-cleansed.parquet";

        var duckDBConnection = new DuckDBConnection("Data Source=:memory:");
        duckDBConnection.Open();

        using var command = duckDBConnection.CreateCommand();

        command.CommandText = "INSTALL spatial; LOAD spatial;";
        command.ExecuteNonQuery();

        command.CommandText = $@"CREATE TABLE world AS SELECT * REPLACE (ST_GeomFromWKB(geo_shape) as geo_shape) FROM '{world}';";
        command.ExecuteNonQuery();

        command.CommandText = $@"CREATE TABLE state AS SELECT * REPLACE (ST_GeomFromWKB(geo_shape) as geo_shape) FROM '{state}';";
        command.ExecuteNonQuery();

        command.CommandText = $@"CREATE TABLE county AS SELECT * REPLACE (ST_GeomFromWKB(geo_shape) as geo_shape) FROM '{county}';";
        command.ExecuteNonQuery();

        command.CommandText = $@"CREATE TABLE zcta AS SELECT * REPLACE (ST_GeomFromWKB(geo_shape) as geo_shape) FROM '{zcta}';";
        command.ExecuteNonQuery();

        command.CommandText = $@"CREATE TABLE place AS SELECT * REPLACE (ST_GeomFromWKB(geo_shape) as geo_shape) FROM '{place}';";
        command.ExecuteNonQuery();

        return duckDBConnection;
    }


    public static DuckDBConnection GetSpatialDb2()
    {
        var duckDBConnection = new DuckDBConnection(@"Data Source=C:\temp\duckdb\file.db");
        duckDBConnection.Open();

        using var command = duckDBConnection.CreateCommand();
        command.CommandText = "INSTALL spatial; LOAD spatial;";
        command.ExecuteNonQuery();

        return duckDBConnection;
    }

    private static DuckDBConnection GetOvertureDb()
    {
        //var duckDBConnection = new DuckDBConnection($@"Data Source=C:\temp\overture\duck.db");
        var duckDBConnection = new DuckDBConnection("Data Source=:memory:");
        duckDBConnection.Open();

        using var command = duckDBConnection.CreateCommand();

        command.CommandText = "INSTALL spatial; LOAD spatial;";
        command.ExecuteNonQuery();

        return duckDBConnection;
    }
    private static async Task TestDuckDB()
    {
        using var duckDBConnection = DuckDbSupport.GetSpatialDb();
        using var command = duckDBConnection.CreateCommand();

        //command.CommandText = "SELECT * FROM duckdb_settings() WHERE name = 'access_mode'";
        command.CommandText = $@"SELECT *, ST_AREA(geo_shape) as geo_area, ST_AsGeoJSON(ST_Intersection(geo_shape, geo_shape)) as is_intersection FROM world";

        var reader = await command.ExecuteReaderAsync();

        PrintQueryResults(reader);

        static void PrintQueryResults(DbDataReader queryResult)
        {
            for (var index = 0; index < queryResult.FieldCount; index++)
            {
                var column = queryResult.GetName(index);
                Console.Write($"{column} ");
            }

            Console.WriteLine();

            while (queryResult.Read())
            {
                //for (int ordinal = 0; ordinal < queryResult.FieldCount; ordinal++)
                //{
                //    var val = queryResult.GetInt32(ordinal);
                //    Console.Write(val);
                //    Console.Write(" ");
                //}

                var strOrig = queryResult.GetValue(queryResult.GetOrdinal("geo_shape"));
                var strIntersect = queryResult.GetValue(queryResult.GetOrdinal("is_intersection"));

                //var str = strOrig as System.IO.UnmanagedMemoryStream;
                //var bytes = new byte[str.Length];
                //str.ReadExactlyAsync(bytes, 0, (int)str.Length);
                Console.WriteLine($@"iso3: {queryResult.GetValue(queryResult.GetOrdinal("iso3"))} -- intersection: {strIntersect}");
            }
        }
    }

    public static async Task TestDuckDBTooSlow()
    {
        var conn = DuckDbSupport.InstanceOverture;
        using var command = conn.CreateCommand();

        command.CommandText = "CALL load_aws_credentials();";
        var reader = await command.ExecuteReaderAsync();

        command.CommandText = $@"SELECT *
        FROM read_parquet('s3://overturemaps-us-west-2/release/2024-07-22.0/theme=addresses/type=*/*', filename=true, hive_partitioning=1)
        where
          bbox.xmin > -179.0 
          and bbox.xmax <  -65.7 
          and bbox.ymin > 18.0 
          and bbox.ymax < 71.6
        LIMIT 5";
        reader = await command.ExecuteReaderAsync();


        PrintQueryResults(reader);

        static void PrintQueryResults(DbDataReader queryResult)
        {
            int fieldCount = queryResult.FieldCount;
            for (var index = 0; index < fieldCount; index++)
            {
                var column = queryResult.GetName(index);
                Console.Write($"{column}");
            }
            Console.WriteLine();

            while (queryResult.Read())
            {
                for (int i = 0; i < fieldCount; i++)
                    Console.Write(queryResult.GetValue(i));

                Console.WriteLine();
            }
        }
    }

}