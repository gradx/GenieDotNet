namespace Genie.Common.Types;

public record LocationAddress
{
    public string? Line1Address { get; set; }
    public string? Line2Address { get; set; }
    public string? MunicipalityName { get; set; }
    public string? CountryCode { get; set; }
    public string? StateCode { get; set; }
    public string? PostalCode { get; set; }
}