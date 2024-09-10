using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using System.Security.Cryptography;

namespace Genie.Common.Crypto.Adapters.Rsa;
// Signing
// Encryption
public sealed class Rsa1024Adapter : RsaBaseAdapter, IAsymmetricSignature<RSAParameters>, IAsymmetricCipher<RSA>
{
    private static readonly Lazy<Rsa1024Adapter> _instance = new(() => new());
    private Rsa1024Adapter() : base(1024) { }
    public static Rsa1024Adapter Instance { get { return _instance.Value; } }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Import<T>(k) : ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }
}