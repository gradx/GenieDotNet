namespace Genie.Common.Types;
public record Device : CosmosBase
{
    public string? Name { get; set; }
    public string? Model { get; set; }
    public string? Vendor { get; set; }
    public string? SystemName { get; set; }
    public string? SystemVersion { get; set; }
    public string? Handle { get; set; }
    public enum Platform
    {
        Default = 0,
        iOS = 1,
        Android = 2
    }
}