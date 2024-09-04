using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using Org.BouncyCastle.Pqc.Crypto.Utilities;
using Org.BouncyCastle.Security;

namespace Genie.Common.Crypto.Adapters.Pqc;

// Signing
public class DilithiumAdapter : IAsymmetricBase, IAsymmetricSignature<ICipherParameters>
{
    private static readonly Lazy<DilithiumAdapter> _instance = new(() => new());
    private DilithiumAdapter() { }
    public static DilithiumAdapter Instance { get { return _instance.Value; } }
    private static readonly DilithiumParameters Params = DilithiumParameters.Dilithium3;

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public static AsymmetricCipherKeyPair GenerateKeyPair()
    {
        return GenerateKeyPair(Params);
    }

    public static AsymmetricCipherKeyPair GenerateKeyPair(DilithiumParameters param)
    {
        var gen = new DilithiumKeyPairGenerator();
        gen.Init(new DilithiumKeyGenerationParameters(new SecureRandom(), param));

        return gen.GenerateKeyPair();
    }

    public byte[] Sign(byte[] data, ICipherParameters key)
    {
        var signer = new DilithiumSigner();
        signer.Init(true, key);

        return signer.GenerateSignature(data);
    }


    public bool Verify(byte[] data, byte[] signature, ICipherParameters key)
    {
        var verifier = new DilithiumSigner();
        if (key is DilithiumPrivateKeyParameters @private)
            verifier.Init(false, @private.GetPublicKeyParameters());
        else
            verifier.Init(false, key);


        return verifier.VerifySignature(data, signature);
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Instance.Import<T>(k) : Instance.ImportX509<T>(k.X509!);
    }

    public static AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        return k.IsPrivate ? PqcPrivateKeyFactory.CreateKey(k.X509) : PqcPublicKeyFactory.CreateKey(k.X509);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public static AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        return PqcPublicKeyFactory.CreateKey(x509);
    }

    public byte[] Export(ICipherParameters key, bool isPrivate)
    {
        if (key is DilithiumPrivateKeyParameters parameters)
            return PqcPrivateKeyInfoFactory.CreatePrivateKeyInfo(parameters).GetEncoded();
        else
            return PqcSubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo((DilithiumPublicKeyParameters)key).GetEncoded();
    }

    //public static X509Certificate2 ExportX509PublicCertificate(AsymmetricCipherKeyPair kp, string issuer)
    //{
    //    X509V3CertificateGenerator genX509 = new();
    //    genX509.SetPublicKey(kp.Public);
    //    genX509.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
    //    genX509.SetIssuerDN(new X509Name("CN=" + issuer));
    //    genX509.SetSubjectDN(new X509Name("CN=" + issuer));
    //    genX509.SetNotBefore(DateTime.UtcNow);
    //    genX509.SetNotAfter(DateTime.UtcNow.AddYears(3));
    //    genX509.AddExtension(X509Extensions.KeyUsage, false, new KeyUsage(KeyUsage.KeyCertSign));
    //    genX509.AddExtension(X509Extensions.BasicConstraints, false, new BasicConstraints(false));

    //    var ecSign = new ECKeyPairGenerator();
    //    var kg = new KeyGenerationParameters(new SecureRandom(), 521);
    //    ecSign.Init(kg);
    //    ISignatureFactory sigFac = new Asn1SignatureFactory("Sha512WithECDSA", ecSign.GenerateKeyPair().Private);
    //    return new X509Certificate2(genX509.Generate(sigFac).GetEncoded());
    //}
}