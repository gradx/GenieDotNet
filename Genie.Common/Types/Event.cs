
namespace Genie.Common.Types;
public record Event : CosmosBase
{
    public string Code { get; set; } = "";
    public Guid DeviceId { get; set; }
    public string? IpAddressSource { get; set; }
    public string? IpAddressDestination { get; set; }
    public Coordinate? Origin { get; set; }
    public string? Info { get; set; }
    public DateTime EventDate { get; set; }
}