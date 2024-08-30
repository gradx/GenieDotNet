
namespace Genie.Common.Types.Core;

public record SignedParty : BaseRequest
{
    public string PartyId { get; set; } = "";
    public string Signature { get; set; } = "";
    public enum SignedType
    {
        Party = 0,
        Channel = 1
    }
    public SignedType PartyType { get; set; }
    public string? GeoCryptoKeyId { get; set; }

}
