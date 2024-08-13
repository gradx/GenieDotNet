using System.ComponentModel;

namespace Genie.Common.Types;

public record SealedEnvelope : GeoCryptoKey
{
    public string? Hkdf { get; set; }
    public string? Data { get; set; }
    public string? Nonce { get; set; }
    public string? Tag { get; set; }
    public enum CipherType
    {
        None = 0,
        AES = 1,
        ChaChaPoly = 2
    }
    public CipherType Cipher { get; set; }
}