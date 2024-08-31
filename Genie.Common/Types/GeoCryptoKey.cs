using Elastic.Clients.Elasticsearch.Ingest;

namespace Genie.Common.Types;

public record GeoCryptoKey
{
    public string? Id { get; set; }
    public byte[]? X509 { get; set; }
    public enum CryptoKeyType
    {
        X25519 = 0,
        Ed25519 = 1,
        Rsa = 2,
        Secp256k1 = 3,
        Secp521r1Signing = 4,
        Secp521r1Agreement = 5
    }
    public CryptoKeyType KeyType { get; set; }
    public enum Usage
    {
        Signing = 0,
        Agreement = 1
    }
    public Usage KeyUsage { get; set; }
    public bool IsPrivate { get; set; }
}