using Genie.Common.Types;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Genie.Common.Crypto.Adapters.Nist;

public abstract class SecpBaseSigningAdapter(string oid, int keySize)
{
    public ECDsa GenerateKeyPair()
    {
        var key = ECDsa.Create();
        key.GenerateKey(ECCurve.CreateFromValue(oid));
        key.KeySize = keySize;
        return key;
    }

    public byte[] Sign(byte[] data, ECDsa key)
    {
        var hasher = SHA256.Create();
        var hash = hasher.ComputeHash(data);
        return key.SignHash(hash);

        //return key.SignData(data, HashAlgorithmName.SHA256);
    }

    public bool Verify(byte[] data, byte[] signature, ECDsa key)
    {
        var hasher = SHA256.Create();
        var hash = hasher.ComputeHash(data);
        return key.VerifyHash(hash, signature);

        //return key.VerifyData(data, signature, HashAlgorithmName.SHA256);
    }

    public static ECDsa? Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            var r = ECDsa.Create();
            r.ImportPkcs8PrivateKey(k.X509!, out int _);
            return r;
        }

        else
            return ImportX509(k.X509!);
    }

    public static ECDsa? ImportX509(byte[] x509)
    {
        var cert = new X509Certificate2(x509);
        return cert.GetECDsaPublicKey();
    }


    public byte[] Export(ECDsa key, bool isPrivate)
    {
        return isPrivate ? key.ExportPkcs8PrivateKey() : key.ExportSubjectPublicKeyInfo();
    }

    public static X509Certificate2 ExportX509PublicCertificate(ECDsa key, string issuer)
    {
        CertificateRequest request = new("CN=" + issuer, key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: false));
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

        // If it was for TLS, then Subject Alternative Names and Extended (Enhanced) Key Usages would also be useful.
        DateTimeOffset start = DateTimeOffset.UtcNow;
        var cert = request.CreateSelfSigned(notBefore: start, notAfter: start.AddYears(3));

        return cert;
    }
}