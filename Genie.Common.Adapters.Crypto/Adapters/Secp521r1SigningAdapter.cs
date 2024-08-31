using Genie.Common.Types;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Genie.Common.Crypto.Adapters;
public class Secp521r1SigningAdapter : IAsymmetricBase, IAsymmetricSignature<ECDsa>
{
    private static readonly Lazy<Secp521r1SigningAdapter> _instance = new(() => new());
    private Secp521r1SigningAdapter() { }
    public static Secp521r1SigningAdapter Instance { get { return _instance.Value; } }
    const string oid = "1.3.132.0.35";

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public ECDsa GenerateKeyPair()
    {
        var key = ECDsa.Create();
        key.GenerateKey(ECCurve.CreateFromValue(oid));
        key.KeySize = 521;
        return key;
    }

    public byte[] Sign(byte[] data, ECDsa key)
    {
        return key.SignData(data, HashAlgorithmName.SHA512);
    }

    public bool Verify(byte[] data, byte[] signature, ECDsa key)
    {
        return key.VerifyData(data, signature, HashAlgorithmName.SHA512);
    }
    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Import<T>(k) : ImportX509<T>(k.X509!);
    }

    public ECDsa? Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            var r = ECDsa.Create();
            r.ImportPkcs8PrivateKey(k.X509!, out int read);
            return r;
        }

        else
            return ImportX509(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public ECDsa? ImportX509(byte[] x509)
    {
        var cert = new X509Certificate2(x509);
        return cert.GetECDsaPublicKey();
    }


    public byte[] Export(ECDsa key, bool isPrivate)
    {
        return isPrivate ? key.ExportPkcs8PrivateKey() : key.ExportSubjectPublicKeyInfo();
    }

    public X509Certificate2 ExportX509Certificate(ECDsa key, string issuer)
    {
        CertificateRequest request = new("CN=" + issuer, key, HashAlgorithmName.SHA512);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: false));
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

        // If it was for TLS, then Subject Alternative Names and Extended (Enhanced) Key Usages would also be useful.
        DateTimeOffset start = DateTimeOffset.UtcNow;
        var cert = request.CreateSelfSigned(notBefore: start, notAfter: start.AddYears(3));

        return cert;
    }
}