using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;

namespace Genie.Common.Crypto.Adapters.Curve25519;

// Signing
public class Ed448Adapter : IAsymmetricBase, IAsymmetricSignature<ICipherParameters>
{
    private static readonly Lazy<Ed448Adapter> _instance = new(() => new());
    private Ed448Adapter() { }
    public static Ed448Adapter Instance { get { return _instance.Value; } }

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public static AsymmetricCipherKeyPair GenerateKeyPair()
    {
        var gen = new Ed448KeyPairGenerator();
        gen.Init(new Ed448KeyGenerationParameters(new()));
        return gen.GenerateKeyPair();
    }

    public byte[] Sign(byte[] data, ICipherParameters key)
    {
        var signer = SignerUtilities.GetSigner("Ed448");
        signer.Init(true, key);
        signer.BlockUpdate(data, 0, data.Length);

        return signer.GenerateSignature();
    }

    public bool Verify(byte[] data, byte[] signature, ICipherParameters key)
    {
        var verifier = SignerUtilities.GetSigner("Ed448");
        if (key is Ed448PrivateKeyParameters @private)
            verifier.Init(false, @private.GeneratePublicKey());
        else
            verifier.Init(false, key);
        verifier.BlockUpdate(data, 0, data.Length);

        return verifier.VerifySignature(signature);
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Instance.Import<T>(k) : Instance.ImportX509<T>(k.X509!);
    }

    public static AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        return k.IsPrivate ? new Ed448PrivateKeyParameters(k.X509, 0) : ImportX509(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public static AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        return new Ed448PublicKeyParameters(new X509Certificate2(x509).GetPublicKey(), 0);
    }

    public byte[] Export(ICipherParameters key, bool isPrivate)
    {
        return key is Ed448PrivateKeyParameters @private ? @private.GetEncoded() : ((Ed448PublicKeyParameters)key).GetEncoded();
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
        genX509.AddExtension(X509Extensions.KeyUsage, false, new KeyUsage(KeyUsage.KeyCertSign));
        genX509.AddExtension(X509Extensions.BasicConstraints, false, new BasicConstraints(false));

        var ecSign = new ECKeyPairGenerator();
        var kg = new KeyGenerationParameters(new SecureRandom(), 521);
        ecSign.Init(kg);
        ISignatureFactory sigFac = new Asn1SignatureFactory("Sha512WithECDSA", ecSign.GenerateKeyPair().Private);
        return new X509Certificate2(genX509.Generate(sigFac).GetEncoded());
    }
}