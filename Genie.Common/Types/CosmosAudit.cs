namespace Genie.Common.Types;

public record CosmosAudit
{
    public string? Version { get; set; } = "";

    public string? Signature { get; set; } = "";
}