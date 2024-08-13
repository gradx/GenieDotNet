﻿using Genie.Common.Types;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Genie.Common.Crypto.Adapters;
public class Secp521r1AgreementAdapter : IAsymmetricBase, IAsymmetricCipher<HkdfParameters>
{
    private static readonly Lazy<Secp521r1AgreementAdapter> _instance = new(() => new());
    private Secp521r1AgreementAdapter() { }
    public static Secp521r1AgreementAdapter Instance { get { return _instance.Value; } }

    const string oid = "1.3.132.0.35"; // externally with ECDH: 1.2.840.10045.4.3.4

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public ECDiffieHellman GenerateKeyPair()
    {
        var key = ECDiffieHellman.Create(ECCurve.CreateFromValue(oid));
        key.KeySize = 521;
        return key;
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Import<T>(k) : ImportX509<T>(Convert.FromBase64String(k.Key!));
    }

    public ECDiffieHellman? Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            var r = ECDiffieHellman.Create();
            r.ImportPkcs8PrivateKey(Convert.FromBase64String(k.Key!), out int read);
            return r;
        }
        else
            return ImportX509(Convert.FromBase64String(k.Key!));
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public ECDiffieHellman? ImportX509(byte[] x509)
    {
        return new X509Certificate2(x509).GetECDiffieHellmanPublicKey();
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


    public byte[] Export(ECDiffieHellman key, bool isPrivate)
    {
        return isPrivate ? key.ExportPkcs8PrivateKey() : key.ExportSubjectPublicKeyInfo();
    }

    public X509Certificate2 ExportX509Certificate(ECDiffieHellman ecdh, string issuer)
    {

        PublicKey pubKey = new(new Oid(oid), new AsnEncodedData(oid, new byte[] { 05, 00 }), new AsnEncodedData(ecdh.ExportSubjectPublicKeyInfo()));
        CertificateRequest request = new(new X500DistinguishedName("CN=" + issuer), pubKey, HashAlgorithmName.SHA512);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyAgreement, critical: false));
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

        var signing = ECDsa.Create(ECCurve.CreateFromValue(oid));
        DateTimeOffset start = DateTimeOffset.UtcNow;
        var cert = request.Create(new X500DistinguishedName("CN=" + issuer), X509SignatureGenerator.CreateForECDsa(signing), start, start.AddYears(3), Encoding.UTF8.GetBytes("Serial No."));

        return cert;
    }
}