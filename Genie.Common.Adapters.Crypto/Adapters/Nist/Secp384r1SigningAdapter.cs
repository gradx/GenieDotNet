using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using System.Security.Cryptography;


namespace Genie.Common.Crypto.Adapters.Nist;

public class Secp384r1SigningAdapter : SecpBaseSigningAdapter, IAsymmetricBase, IAsymmetricSignature<ECDsa>
{
    private static readonly Lazy<Secp384r1SigningAdapter> _instance = new(() => new());
    private Secp384r1SigningAdapter() : base(oid, 384) { }
    public static Secp384r1SigningAdapter Instance { get { return _instance.Value; } }
    const string oid = "1.3.132.0.34";

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