using Org.BouncyCastle.Crypto.Parameters;

namespace Genie.Common.Crypto.Adapters.Kdf;
public class HkdfParameters
{
    public HkdfParameters(ECPrivateKeyParameters privKey, ECPublicKeyParameters pubKey, byte[] hkdf, byte[] nonce)
    {
        Private = privKey;
        Public = pubKey;
        Hkdf = hkdf;
        Nonce = nonce;
    }

    public ECPrivateKeyParameters Private { get; }
    public ECPublicKeyParameters Public { get; }
    public byte[] Hkdf { get; }
    public byte[] Nonce { get; }
}