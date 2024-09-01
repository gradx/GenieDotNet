using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;


namespace Genie.Common.Crypto.Adapters.Nist;
// Signing
public class Secp256k1Adapter : IAsymmetricBase, IAsymmetricSignature<ICipherParameters>
{
    private static readonly Lazy<Secp256k1Adapter> _instance = new(() => new());
    private Secp256k1Adapter() { }
    public static Secp256k1Adapter Instance { get { return _instance.Value; } }

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public static AsymmetricCipherKeyPair GenerateKeyPair()
    {
        var ecParams = ECNamedCurveTable.GetByName("secp256k1");
        var domainParameters = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H);
        var keyGenParams = new ECKeyGenerationParameters(domainParameters, new SecureRandom());
        var generator = new ECKeyPairGenerator();
        generator.Init(keyGenParams);
        return generator.GenerateKeyPair();
    }

    public byte[] Sign(byte[] data, ICipherParameters key)
    {
        var signer = new ECDsaSigner();
        signer.Init(true, key);
        var signature = signer.GenerateSignature(data);
        return [.. signature[0].ToByteArrayUnsigned(), .. signature[1].ToByteArrayUnsigned()];
    }

    public bool Verify(byte[] data, byte[] signature, ICipherParameters key)
    {
        var verifier = new ECDsaSigner();
        if (key is ECPrivateKeyParameters @private)
            verifier.Init(false, new ECPublicKeyParameters(@private.Parameters.G.Multiply(@private.D), @private.Parameters));
        else
            verifier.Init(false, key);

        //https://www.py4u.net/discuss/185847
        return verifier.VerifySignature(data, new BigInteger(1, signature, 0, 32), new BigInteger(1, signature, 32, 32));
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Instance.Import<T>(k) : Instance.ImportX509<T>(k.X509!);
    }

    public static AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            var ecP = ECNamedCurveTable.GetByName("secp256k1");
            var ecSpec = new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H);
            return new ECPrivateKeyParameters(new BigInteger(k.X509!), ecSpec);
        }
        else
            return ImportX509(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public static AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        var ecP = ECNamedCurveTable.GetByName("secp256k1");
        var ecSpec = new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H);
        var publicPoint = ecP.Curve.DecodePoint(new X509Certificate2(x509).GetPublicKey());
        return new ECPublicKeyParameters(publicPoint, ecSpec);
    }

    public byte[] Export(ICipherParameters key, bool isPrivate)
    {
        return isPrivate ? ((ECPrivateKeyParameters)key).D.ToByteArray() : ((ECPublicKeyParameters)key).Q.GetEncoded();
    }

    public static X509Certificate2 ExportX509Certificate(AsymmetricCipherKeyPair kp, string issuer)
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
        var sigFac = new Asn1SignatureFactory("Sha512WithECDSA", ecSign.GenerateKeyPair().Private);
        return new X509Certificate2(genX509.Generate(sigFac).GetEncoded());
    }
}