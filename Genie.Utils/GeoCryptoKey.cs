
namespace Genie.Common.Types;

public record GeoCryptoKey
{
    public string? Id { get; set; }
    public byte[]? X509 { get; set; }
    public enum CryptoKeyType
    {
        X25519 = 0,
        Ed25519 = 1,
        Rsa_2048 = 2,
        Rsa_3072 = 3,
        Rsa_4096 = 4,

        Secp256k1 = 10,
        Secp256r1 = 11,
        Secp384r1 = 12,
        Secp521r1 = 13,
        Kyber512 = 14,
        Kyber768 = 15,
        Kyber1024 = 16,
        Dilithium2 = 17,
        Dilithium3 = 18,
        Dilithium5 = 19
    }
    public CryptoKeyType KeyType { get; set; }
    public enum Usage
    {
        Signing = 0,
        Agreement = 1
    }
    public Usage KeyUsage { get; set; }
    public bool IsPrivate { get; set; }
    public byte[]? PqcE { get; set; }
}