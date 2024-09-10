using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using System.Security.Cryptography;

namespace Genie.Common.Crypto.Adapters.Rsa;
// Signing
// Encryption
public sealed class Rsa4096Adapter : RsaBaseAdapter, IAsymmetricSignature<RSAParameters>, IAsymmetricCipher<RSA>
{
    private static readonly Lazy<Rsa4096Adapter> _instance = new(() => new());
    private Rsa4096Adapter() : base(4096) { }
    public static Rsa4096Adapter Instance { get { return _instance.Value; } }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Import<T>(k) : ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }
}