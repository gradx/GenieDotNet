using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Crypto.Adapters.Kdf;
using Genie.Common.Types;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;
using HkdfParameters = Genie.Common.Crypto.Adapters.Kdf.HkdfParameters;

namespace Genie.Common.Crypto.Adapters.Curve25519;
// Key Agreement
public class X25519Adapter : IAsymmetricBase, IAsymmetricCipher<HkdfParameters>
{
    private static readonly Lazy<X25519Adapter> _instance = new(() => new());
    private X25519Adapter() { }
    public static X25519Adapter Instance { get { return _instance.Value; } }

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public static AsymmetricCipherKeyPair GenerateKeyPair()
    {
        var genX = new X25519KeyPairGenerator();
        var kg = new KeyGenerationParameters(new SecureRandom(), 521);
        genX.Init(kg);

        return genX.GenerateKeyPair();
    }

    public T Import<T>(GeoCryptoKey k)
    {
        if (k.IsPrivate)
            return Instance.Import<T>(k);
        else
            return Instance.ImportX509<T>(new X509Certificate2(k.X509!).GetPublicKey());
    }

    public static AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
            return new X25519PrivateKeyParameters(k.X509, 0);
        else
            return ImportX509(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public static AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        return new X25519PublicKeyParameters(new X509Certificate2(x509).GetPublicKey(), 0);
    }

    public static byte[] GenerateSecret(X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey)
    {
        var secret = new byte[32];
        privateKey.GenerateSecret(publicKey, secret, 0);
        return secret;
    }

    public byte[] Encrypt(HkdfParameters provider, byte[] data)
    {
        return Encryption(true, provider, data);
    }

    public byte[] Decrypt(HkdfParameters provider, byte[] data)
    {
        return Encryption(false, provider, data);
    }

    private static byte[] Encryption(bool forEncryption, HkdfParameters provider, byte[] data)
    {
        var gcm = new GcmBlockCipher(new AesEngine());
        var ies = new IesEngine(new ECDHBasicAgreement(), new Kdf2BytesGenerator(new Sha512Digest()),
            new HMac(new Sha512Digest()), new PaddedBufferedBlockCipher(gcm.UnderlyingCipher, new ZeroBytePadding()));

        ies.Init(forEncryption, provider.Private, provider.Public, new IesWithCipherParameters(provider.Hkdf, provider.Nonce, 512, 256));
        return ies.ProcessBlock(data, 0, data.Length);
    }

    public static byte[] Export(AsymmetricKeyParameter key)
    {
        if (key is X25519PrivateKeyParameters parameters)
            return parameters.GetEncoded();
        else
            return ((X25519PublicKeyParameters)key).GetEncoded();
    }

    public static X509Certificate2 ExportX509PublicCertificate(AsymmetricCipherKeyPair kp, string issuer)
    {

        X509V3CertificateGenerator genX509 = new();
        genX509.SetPublicKey(kp.Public);
        genX509.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
        genX509.SetIssuerDN(new X509Name("CN=" + issuer));
        genX509.SetSubjectDN(new X509Name("CN=" + issuer));
        genX509.SetNotBefore(DateTime.UtcNow);
        genX509.SetNotAfter(DateTime.UtcNow.AddYears(3));
        genX509.AddExtension(X509Extensions.KeyUsage, false, new KeyUsage(KeyUsage.KeyAgreement));
        genX509.AddExtension(X509Extensions.BasicConstraints, false, new BasicConstraints(false));

        var ecSign = new ECKeyPairGenerator();
        var kg = new KeyGenerationParameters(new SecureRandom(), 521);
        ecSign.Init(kg);
        ISignatureFactory sigFac = new Asn1SignatureFactory("Sha512WithECDSA", ecSign.GenerateKeyPair().Private);
        return new X509Certificate2(genX509.Generate(sigFac).GetEncoded());
    }

    public static X25519PublicKeyParameters GetX25519PublicKeyParameters(X509Certificate2 c)
    {
        return new X25519PublicKeyParameters(c.PublicKey.EncodedKeyValue.RawData, 0);
    }
}