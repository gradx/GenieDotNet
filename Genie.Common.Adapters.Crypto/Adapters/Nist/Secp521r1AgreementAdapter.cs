using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using HkdfParameters = Genie.Common.Crypto.Adapters.Kdf.HkdfParameters;

namespace Genie.Common.Crypto.Adapters.Nist;

public class Secp521r1AgreementAdapter : SecpBaseAgreementAdapter, IAsymmetricBase, IAsymmetricCipher<HkdfParameters>
{
    private static readonly Lazy<Secp521r1AgreementAdapter> _instance = new(() => new());
    private Secp521r1AgreementAdapter() : base(oid, 521) { }
    public static Secp521r1AgreementAdapter Instance { get { return _instance.Value; } }

    const string oid = "1.3.132.0.35"; // externally with ECDH: 1.2.840.10045.4.3.4

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


    private static byte[] Encryption(bool forEncryption, HkdfParameters provider, byte[] data)
    {
        var gcm = new GcmBlockCipher(new AesEngine());
        var ies = new IesEngine(new ECDHBasicAgreement(), new Kdf2BytesGenerator(new Sha512Digest()),
            new HMac(new Sha512Digest()), new PaddedBufferedBlockCipher(gcm.UnderlyingCipher, new ZeroBytePadding()));

        ies.Init(forEncryption, provider.Private, provider.Public, new IesWithCipherParameters(provider.Hkdf, provider.Nonce, 512, 256));
        return ies.ProcessBlock(data, 0, data.Length);
    }
}