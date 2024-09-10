using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using System.Security.Cryptography;

namespace Genie.Common.Crypto.Adapters.Rsa;
// Signing
// Encryption
public sealed class Rsa2048Adapter : RsaBaseAdapter, IAsymmetricSignature<RSAParameters>, IAsymmetricCipher<RSA>
{
    private static readonly Lazy<Rsa2048Adapter> _instance = new(() => new());
    private Rsa2048Adapter() : base(2048) { }
    public static Rsa2048Adapter Instance { get { return _instance.Value; } }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Import<T>(k) : ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }
}