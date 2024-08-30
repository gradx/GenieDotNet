
namespace Genie.Common.Types;

public record PartyRequest : BaseRequest
{
    public Party? Party { get; set; }
}

