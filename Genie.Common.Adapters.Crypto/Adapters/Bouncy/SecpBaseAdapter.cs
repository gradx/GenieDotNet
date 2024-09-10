using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Math;
using Genie.Common.Types;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.Pkcs;

namespace Genie.Common.Crypto.Adapters.Bouncy;

public abstract class SecpBaseAdapter(string curveName)
{
    public AsymmetricCipherKeyPair GenerateKeyPair()
    {
        var ecParams = ECNamedCurveTable.GetByName(curveName);
        var domainParameters = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H);
        var keyGenParams = new ECKeyGenerationParameters(domainParameters, new SecureRandom());
        var generator = new ECKeyPairGenerator();
        generator.Init(keyGenParams);
        return generator.GenerateKeyPair();
    }

    public static byte[] CreateSecret(ECPrivateKeyParameters privateKey, ECPublicKeyParameters publicKey)
    {
        return publicKey.Q.Multiply(privateKey.D).Normalize().AffineXCoord.GetEncoded();
    }

    public BigInteger[] SignBigInteger(byte[] data, ICipherParameters key)
    {
        //new HMacDsaKCalculator(new Sha256Digest())
        var signer = new ECDsaSigner();
        signer.Init(true, key);
        return signer.GenerateSignature(data);

    }

    private static void ConvertSignature(ref byte[] part1, ref byte[] part2, int targetSize)
    {
        if (part1.Length > targetSize)
            part1 = part1.Skip(part1.Length - targetSize).ToArray();
        else if (part1.Length < targetSize)
        {
            while (part1.Length < targetSize)
                part1 = part1.Prepend(new byte()).ToArray();
        }

        if (part2.Length > targetSize)
            part2 = part2.Skip(part2.Length - targetSize).ToArray();
        else if (part2.Length < targetSize)
        {
            while (part2.Length < targetSize)
                part2 = part2.Prepend(new byte()).ToArray();
        }
    }

    public byte[] Sign(byte[] data, ICipherParameters key)
    {
        var signer = new ECDsaSigner();
        signer.Init(true, key);
        var signature = signer.GenerateSignature(data);
        //var signature = SignBigInteger(data, key);

        var part1 = signature[0].ToByteArray();
        var part2 = signature[1].ToByteArray();
        if (signer.Order.BitLength == 256)
            ConvertSignature(ref part1, ref part2, 32);
        else if (signer.Order.BitLength == 384)
            ConvertSignature(ref part1, ref part2, 48);
        else if (signer.Order.BitLength == 521)
            ConvertSignature(ref part1, ref part2, 66);

        byte[] result = [.. part1, .. part2];
        return result;

        //var verifier = new ECDsaSigner();
        //var @private = (ECPrivateKeyParameters)key;
        //verifier.Init(false, new ECPublicKeyParameters(@private.Parameters.G.Multiply(@private.D), @private.Parameters));
        //var verified = verifier.VerifySignature(data, signature[0], signature[1]);

        //return result;
    }

    public bool Verify(byte[] data, byte[] signature, ICipherParameters key)
    {
        //var verifier = SignerUtilities.GetSigner("SHA-256withECDSA");
        //if (key is ECPrivateKeyParameters @private)
        //    verifier.Init(false, new ECPublicKeyParameters(@private.Parameters.G.Multiply(@private.D), @private.Parameters));
        //else
        //    verifier.Init(false, key);

        //verifier.BlockUpdate(data);
        //return verifier.VerifySignature(signature);

        ////https://www.py4u.net/discuss/185847

        var length = signature.Length / 2;

        var verifier = new ECDsaSigner();
        if (key is ECPrivateKeyParameters @private)
            verifier.Init(false, new ECPublicKeyParameters(@private.Parameters.G.Multiply(@private.D), @private.Parameters));
        else
            verifier.Init(false, key);

        //var bigInt1 = new BigInteger(signature.Take(length).ToArray());
        //var bigInt2 = new BigInteger(signature.TakeLast(length).ToArray());

        var bigInt1 = new BigInteger(1, signature, 0, length);
        var bigInt2 = new BigInteger(1, signature, length, length);
        return verifier.VerifySignature(data, bigInt1, bigInt2);
    }


    public virtual AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            //return PrivateKeyFactory.CreateKey(k.X509);

            var ecP = ECNamedCurveTable.GetByName(curveName);
            var ecSpec = new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H);
            return new ECPrivateKeyParameters(new BigInteger(k.X509!), ecSpec);
        }
        else
            return ImportX509(k.X509!);
    }

    public virtual AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        var ecP = ECNamedCurveTable.GetByName(curveName);
        var ecSpec = new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H);
        var publicPoint = ecP.Curve.DecodePoint(new X509Certificate2(x509).GetPublicKey());
        return new ECPublicKeyParameters(publicPoint, ecSpec);
    }

    public virtual byte[] Export(ICipherParameters key, bool isPrivate)
    {
       return isPrivate ? ((ECPrivateKeyParameters)key).D.ToByteArray() : ((ECPublicKeyParameters)key).Q.GetEncoded();
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
        var sigFac = new Asn1SignatureFactory("Sha512WithECDSA", ecSign.GenerateKeyPair().Private);
        return new X509Certificate2(genX509.Generate(sigFac).GetEncoded());
    }
}