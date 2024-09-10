using Genie.Common.Crypto.Adapters.Interfaces;
using Genie.Common.Types;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;

namespace Genie.Common.Crypto.Adapters.Bouncy;
// Signing


public class Secp256k1Adapter : SecpBaseAdapter, IAsymmetricBase, IAsymmetricSignature<ICipherParameters>
{
    private static readonly Lazy<Secp256k1Adapter> _instance = new(() => new());
    private Secp256k1Adapter() : base("secp256k1") { }
    public static Secp256k1Adapter Instance { get { return _instance.Value; } }

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Instance.Import<T>(k) : Instance.ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }
}

public class Secp256r1Adapter : SecpBaseAdapter, IAsymmetricBase, IAsymmetricSignature<ICipherParameters>
{
    private static readonly Lazy<Secp256r1Adapter> _instance = new(() => new());
    private Secp256r1Adapter() : base("secp256r1") { }
    public static Secp256r1Adapter Instance { get { return _instance.Value; } }

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Instance.Import<T>(k) : Instance.ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public override AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            return PrivateKeyFactory.CreateKey(k.X509);
        }
        else
            return ImportX509(k.X509!);
    }

    public override AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        return PublicKeyFactory.CreateKey(x509);
    }

    public override byte[] Export(ICipherParameters key, bool isPrivate)
    {
        return isPrivate ? PrivateKeyInfoFactory.CreatePrivateKeyInfo((ECPrivateKeyParameters)key).GetEncoded() :
            SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo((ECPublicKeyParameters)key).GetEncoded();
    }
}

public class Secp384r1Adapter : SecpBaseAdapter, IAsymmetricBase, IAsymmetricSignature<ICipherParameters>
{
    private static readonly Lazy<Secp384r1Adapter> _instance = new(() => new());
    private Secp384r1Adapter() : base("secp384r1") { }
    public static Secp384r1Adapter Instance { get { return _instance.Value; } }

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Instance.Import<T>(k) : Instance.ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public override AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            return PrivateKeyFactory.CreateKey(k.X509);
        }
        else
            return ImportX509(k.X509!);
    }

    public override AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        return PublicKeyFactory.CreateKey(x509);
    }

    public override byte[] Export(ICipherParameters key, bool isPrivate)
    {
        return isPrivate ? PrivateKeyInfoFactory.CreatePrivateKeyInfo((ECPrivateKeyParameters)key).GetEncoded() :
            SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo((ECPublicKeyParameters)key).GetEncoded();
    }
}

public class Secp521r1Adapter : SecpBaseAdapter, IAsymmetricBase, IAsymmetricSignature<ICipherParameters>
{
    private static readonly Lazy<Secp521r1Adapter> _instance = new(() => new());
    private Secp521r1Adapter() : base("secp521r1") { }
    public static Secp521r1Adapter Instance { get { return _instance.Value; } }

    public T GenerateKeyPair<T>()
    {
        return Instance.GenerateKeyPair<T>();
    }

    public T Import<T>(GeoCryptoKey k)
    {
        return k.IsPrivate ? Instance.Import<T>(k) : Instance.ImportX509<T>(k.X509!);
    }

    public T ImportX509<T>(byte[] x509)
    {
        return Instance.ImportX509<T>(x509);
    }

    public override AsymmetricKeyParameter Import(GeoCryptoKey k)
    {
        if (k.IsPrivate)
        {
            return PrivateKeyFactory.CreateKey(k.X509);
        }
        else
            return ImportX509(k.X509!);
    }

    public override AsymmetricKeyParameter ImportX509(byte[] x509)
    {
        return PublicKeyFactory.CreateKey(x509);
    }

    public override byte[] Export(ICipherParameters key, bool isPrivate)
    {
        return isPrivate ? PrivateKeyInfoFactory.CreatePrivateKeyInfo((ECPrivateKeyParameters)key).GetEncoded() :
            SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo((ECPublicKeyParameters)key).GetEncoded();
    }
}