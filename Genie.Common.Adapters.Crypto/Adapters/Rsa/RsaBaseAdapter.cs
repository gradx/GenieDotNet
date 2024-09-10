using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Genie.Common.Crypto.Adapters.Rsa;
// Signing
// Encryption
public abstract class RsaBaseAdapter(int keySize)
{
    private RSA GetCrypoProvider()
    {
        return RSA.Create(keySize);
    }

    public RSA GenerateKeyPair()
    {
        return GetCrypoProvider();
    }

    public byte[] Sign(byte[] data, RSAParameters key)
    {
        using var p = GetCrypoProvider();
        {
            p.ImportParameters(key);
            return p.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    public bool Verify(byte[] data, byte[] signature, RSAParameters key)
    {
        using var p = GetCrypoProvider();
        {
            p.ImportParameters(key);
            return p.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    public static RSA? Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            var r = RSA.Create();
            r.ImportPkcs8PrivateKey(k.X509!, out int _);
            return r;
        }
        else
        {
            var cert = new X509Certificate2(k.X509!);
            return cert.GetRSAPublicKey();
        }
    }

    public static RSA ImportX509(byte[] x509)
    {
        var r = RSA.Create();
        r.ImportRSAPublicKey(new X509Certificate2(x509).GetPublicKey(), out int _);
        return r;
    }

    public byte[] Export(RSAParameters key, bool isPrivate)
    {
        return isPrivate ? RSA.Create(key).ExportPkcs8PrivateKey() : RSA.Create(key).ExportSubjectPublicKeyInfo();
    }

    public static X509Certificate2 ExportX509Certificate(RSA key, string issuer)
    {
        CertificateRequest request = new("CN=" + issuer, key, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment, critical: false));
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        // If it was for TLS, then Subject Alternative Names and Extended (Enhanced) Key Usages would also be useful.

        DateTimeOffset start = DateTimeOffset.UtcNow;
        return request.CreateSelfSigned(notBefore: start, notAfter: start.AddYears(3));
    }

    public byte[] Encrypt(RSA provider, byte[] data)
    {
        return provider.Encrypt(data, RSAEncryptionPadding.Pkcs1);
    }

    public byte[] Decrypt(RSA provider, byte[] data)
    {
        return provider.Decrypt(data, RSAEncryptionPadding.Pkcs1);
    }
}