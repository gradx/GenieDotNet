using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Genie.Common.Crypto.Adapters.Nist;
public class Secp256r1SigningAdapter : SecpBaseSigningAdapter, IAsymmetricBase, IAsymmetricSignature<ECDsa>
{
    private static readonly Lazy<Secp256r1SigningAdapter> _instance = new(() => new());
    private Secp256r1SigningAdapter() : base(oid, 256) { }
    public static Secp256r1SigningAdapter Instance { get { return _instance.Value; } }
    const string oid = "1.2.840.10045.3.1.7";

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Import<T>(k) : ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }
}