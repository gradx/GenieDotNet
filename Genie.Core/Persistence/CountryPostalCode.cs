using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Core.Persistence;

public class CountryPostalCode
{
    public long Id { get; set; }
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
    public string? PlaceName { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public static CountryPostalCode GetFromReader(DbDataReader reader)
    {
        return new CountryPostalCode
        {
            Id = Convert.ToInt32(reader["id"]),
            CountryCode = reader["country_code"] is DBNull ? null : (string)reader["country_code"],
            PostalCode = reader["postal_code"] is DBNull ? null : (string)reader["postal_code"],
            PlaceName = reader["place_name"] is DBNull ? null : (string)reader["place_name"],
            Latitude = reader["latitude"] is DBNull ? null : (double)reader["latitude"],
            Longitude = reader["longitude"] is DBNull ? null : (double)reader["longitude"]
        };
    }

    public static CountryPostalCode GetCode(int i)
    {
        return new CountryPostalCode
        {
            Id = i,
            CountryCode = StringExtensions.RandomString(new StringBuilder(), 3),
            PostalCode = "60001",
            PlaceName = StringExtensions.RandomString(new StringBuilder(), 50),
            Latitude = 102.01482736,
            Longitude = 204.29482492
        };
    }
}

public class CountryPostalCodeMarten
{
    public string Id { get; set; }
    public string? CountryCode { get; set; }
    public string PostalCode { get; set; }
    public string? PlaceName { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public CountryPostalCodeMarten()
    {

    }
}

public class CountryPostalCodeString
{
    public string Id { get; set; }
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
    public string? PlaceName { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public CountryPostalCodeString()
    {

    }

    public CountryPostalCodeString(CountryPostalCode code)
    {
        Id = code.Id.ToString();
        CountryCode = code.CountryCode;
        PostalCode = code.PostalCode;
        PlaceName = code.PlaceName;
        Latitude = code.Latitude;
        Longitude = code.Longitude;
    }
}


public class CountryPostalCodeCouch : CouchDocument
{
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
    public string? PlaceName { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}