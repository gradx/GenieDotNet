
namespace Genie.Common.Types;

public record Certificate
{
    public List<SealedEnvelope> Keys { get; set; } = [];
    public string? SubscriptionId { get; set; }
    public bool Exportable { get; set; }
}