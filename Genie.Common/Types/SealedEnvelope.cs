using System.ComponentModel;

namespace Genie.Common.Types;

public record SealedEnvelope : GeoCryptoKey
{
    public byte[]? Hkdf { get; set; }
    public byte[]? Data { get; set; }
    public byte[]? Nonce { get; set; }
    public byte[]? Tag { get; set; }
    public enum CipherType
    {
        None = 0,
        AES = 1,
        ChaChaPoly = 2
    }
    public CipherType Cipher { get; set; }
}