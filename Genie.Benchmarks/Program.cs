
using Genie.Benchmarks;
using Genie.Common.Crypto.Adapters.Pqc;
using Genie.Common.Crypto.Adapters.Nist;
using System.Security.Cryptography;
using Genie.Common.Crypto.Adapters.Bouncy;
using Org.BouncyCastle.Crypto.Parameters;
using Genie.Common.Crypto.Adapters.Rsa;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using BenchmarkDotNet.Running;
using Genie.Benchmarks.Benchmarks;


//var test = new PqcBenchmarks();
//test.X25519_Dilithium();
//test.X25519_Ed25519();
//test.Kyber_Ed25519();
//test.Kyber_Dilithium();

//Console.ReadLine();

//var alice = Ed448Adapter.GenerateKeyPair();

//File.WriteAllBytes("AliceEd448.key", Ed448Adapter.Instance.Export(alice.Private, true));
//File.WriteAllBytes("AliceEd448.cer", Ed448Adapter.ExportX509PublicCertificate(alice, "Genie").GetRawCertData());

//var bob = Ed448Adapter.GenerateKeyPair();

//File.WriteAllBytes("BobEd448.key", Ed448Adapter.Instance.Export(bob.Private, true));
//File.WriteAllBytes("BobEd448.cer", Ed448Adapter.ExportX509PublicCertificate(bob, "Genie").GetRawCertData());

//var what = new EncryptionBenchmarks();
//what.GetStats();


//Test256();
//Test384();
//Test521();
//TestBitcoin();
//TestRsa4096();
//TestRsa2048();
//TestRsa1024();


//Test256BouncyIntegration();

var test = new PqcNetworkBenchmarks();
test.Ed448();

//var symm = BenchmarkRunner.Run<SymmetricalBenchmarks>();

var results1 = BenchmarkRunner.Run<SecpBenchmarks>();
//var results2 = BenchmarkRunner.Run<CurveBenchmarks>();
//var results3 = BenchmarkRunner.Run<ModuleLatticeBenchmarks>();
//var results4 = BenchmarkRunner.Run<RsaBenchmarks>();
//var results5 = BenchmarkRunner.Run<NistBenchmarks>();

//var results2 = BenchmarkRunner.Run<PqcNetworkBenchmarks>();

//Test256BouncyIntegration();
//Test384BouncyIntegration();
//Test256BouncyIntegration();
//var test = new ModuleLatticeBenchmarks();

//TestBouncy();


//TestBouncy();

static void TestBouncy()
{
    var alice_1 = Secp256r1Adapter.Instance.GenerateKeyPair();
    var alice_2 = Secp384r1Adapter.Instance.GenerateKeyPair();
    var alice_3 = Secp521r1Adapter.Instance.GenerateKeyPair();


    var export = Secp256r1Adapter.Instance.Export(alice_1.Private, true);
    var import = Secp256r1Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
    {
        IsPrivate = true,
        X509 = export
    });

    var export2 = Secp521r1Adapter.Instance.Export(alice_1.Private, true);
    var import2 = Secp521r1Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
    {
        IsPrivate = true,
        X509 = export
    });

    var alice_public_cert = Secp256r1Adapter.ExportX509PublicCertificate(alice_1, "Genie PKI").RawData;
    var alice_public_import = Secp256r1Adapter.Instance.ImportX509(alice_public_cert);



    var public_export = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo((ECPublicKeyParameters)alice_1.Public).GetEncoded();

    var test = PublicKeyFactory.CreateKey(public_export);

    var export3 = Secp256r1Adapter.Instance.Export(alice_1.Public, false);
    var import3 = Secp256r1Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
    {
        IsPrivate = false,
        X509 = export3
    });

    //var help = PrivateKeyFactory.CreateKey(bytes);
    //File.WriteAllBytes($@"AliceSecp256R1.key", bytes);
    File.WriteAllBytes("AliceSecp256R1.key", Secp256r1Adapter.Instance.Export(alice_1.Private, true));
    File.WriteAllBytes("AliceSecp256R1.cer", Secp256r1Adapter.Instance.Export(alice_1.Public, false));
    File.WriteAllBytes("AliceSecp384R1.key", Secp384r1Adapter.Instance.Export(alice_2.Private, true));
    File.WriteAllBytes("AliceSecp384R1.cer", Secp384r1Adapter.Instance.Export(alice_2.Public, false));
    File.WriteAllBytes("AliceSecp521R1.key", Secp521r1Adapter.Instance.Export(alice_3.Private, true));
    File.WriteAllBytes("AliceSecp521R1.cer", Secp521r1Adapter.Instance.Export(alice_3.Public, false));

    var bob_1 = Secp256r1Adapter.Instance.GenerateKeyPair();
    var bob_2 = Secp384r1Adapter.Instance.GenerateKeyPair();
    var bob_3 = Secp521r1Adapter.Instance.GenerateKeyPair();

    File.WriteAllBytes($@"BobSecp256R1.key", Secp256r1Adapter.Instance.Export(bob_1.Private, true));
    File.WriteAllBytes("BobSecp256R1.cer", Secp256r1Adapter.Instance.Export(bob_1.Public, false));
    File.WriteAllBytes("BobSecp384R1.key", Secp384r1Adapter.Instance.Export(bob_2.Private, true));
    File.WriteAllBytes("BobSecp384R1.cer", Secp384r1Adapter.Instance.Export(bob_2.Public, false));
    File.WriteAllBytes("BobSecp521R1.key", Secp521r1Adapter.Instance.Export(bob_3.Private, true));
    File.WriteAllBytes("BobSecp521R1.cer", Secp521r1Adapter.Instance.Export(bob_3.Public, false));


}

static void TestDilithium() 
{
    var alice_d2 = DilithiumAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium2);
    var alice_d3 = DilithiumAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium3);
    var alice_d5 = DilithiumAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium5);

    File.WriteAllBytes("AliceDilithium2.key", DilithiumAdapter.Instance.Export(alice_d2.Private, true));
    File.WriteAllBytes("AliceDilithium2.cer", DilithiumAdapter.Instance.Export(alice_d2.Public, false));
    File.WriteAllBytes("AliceDilithium3.key", DilithiumAdapter.Instance.Export(alice_d3.Private, true));
    File.WriteAllBytes("AliceDilithium3.cer", DilithiumAdapter.Instance.Export(alice_d3.Public, false));
    File.WriteAllBytes("AliceDilithium5.key", DilithiumAdapter.Instance.Export(alice_d5.Private, true));
    File.WriteAllBytes("AliceDilithium5.cer", DilithiumAdapter.Instance.Export(alice_d5.Public, false));

    var bob_d2 = DilithiumAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium2);
    var bob_d3 = DilithiumAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium3);
    var bob_d5 = DilithiumAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium5);

    File.WriteAllBytes("BobDilithium2.key", DilithiumAdapter.Instance.Export(bob_d2.Private, true));
    File.WriteAllBytes("BobDilithium2.cer", DilithiumAdapter.Instance.Export(bob_d2.Public, false));
    File.WriteAllBytes("BobDilithium3.key", DilithiumAdapter.Instance.Export(bob_d3.Private, true));
    File.WriteAllBytes("BobDilithium3.cer", DilithiumAdapter.Instance.Export(bob_d3.Public, false));
    File.WriteAllBytes("BobDilithium5.key", DilithiumAdapter.Instance.Export(bob_d5.Private, true));
    File.WriteAllBytes("BobDilithium5.cer", DilithiumAdapter.Instance.Export(bob_d5.Public, false));
}

static void TestKyber()
{
    var alice_d2 = KyberAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber.KyberParameters.kyber512);
    var alice_d3 = KyberAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber.KyberParameters.kyber768);
    var alice_d5 = KyberAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber.KyberParameters.kyber1024);

    File.WriteAllBytes("AliceKyber512.key", KyberAdapter.Export(alice_d2.Private));
    File.WriteAllBytes("AliceKyber512.cer", KyberAdapter.Export(alice_d2.Public));
    File.WriteAllBytes("AliceKyber768.key", KyberAdapter.Export(alice_d3.Private));
    File.WriteAllBytes("AliceKyber768.cer", KyberAdapter.Export(alice_d3.Public));
    File.WriteAllBytes("AliceKyber1024.key", KyberAdapter.Export(alice_d5.Private));
    File.WriteAllBytes("AliceKyber1024.cer", KyberAdapter.Export(alice_d5.Public));

    var bob_d2 = KyberAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber.KyberParameters.kyber512);
    var bob_d3 = KyberAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber.KyberParameters.kyber768);
    var bob_d5 = KyberAdapter.GenerateKeyPair(Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber.KyberParameters.kyber1024);

    File.WriteAllBytes("BobKyber512.key", KyberAdapter.Export(bob_d2.Private));
    File.WriteAllBytes("BobKyber512.cer", KyberAdapter.Export(bob_d2.Public));
    File.WriteAllBytes("BobKyber768.key", KyberAdapter.Export(bob_d3.Private));
    File.WriteAllBytes("BobKyber768.cer", KyberAdapter.Export(bob_d3.Public));
    File.WriteAllBytes("BobKyber1024.key", KyberAdapter.Export(bob_d5.Private));
    File.WriteAllBytes("BobKyber1024.cer", KyberAdapter.Export(bob_d5.Public));
}

static void Test521BouncyIntegration()
{
    var data = Utf8StringInterpolation.Utf8String.Format($"help");
    var windows = Secp521r1SigningAdapter.Instance.GenerateKeyPair();
    var hash = SHA256.HashData(data);

    var windows_signature = Secp521r1SigningAdapter.Instance.Sign(data, windows);

    var exchange = ECDiffieHellman.Create();
    exchange.ImportPkcs8PrivateKey(windows.ExportPkcs8PrivateKey(), out int _);

    var bouncy_private_key = PrivateKeyFactory.CreateKey(windows.ExportPkcs8PrivateKey());
    var bouncy_public_key = PublicKeyFactory.CreateKey(windows.ExportSubjectPublicKeyInfo());

    var bouncy_signature = Secp521r1Adapter.Instance.Sign(hash, bouncy_private_key);

    var windows_verify_bouncy = Secp521r1SigningAdapter.Instance.Verify(data, bouncy_signature, windows);
    var windows_verify_windows = Secp521r1SigningAdapter.Instance.Verify(data, windows_signature, windows);

    var bouncy_verify_bouncy = Secp521r1Adapter.Instance.Verify(hash, bouncy_signature, bouncy_public_key);
    var bouncy_verify_windows = Secp521r1Adapter.Instance.Verify(hash, windows_signature, bouncy_public_key);


    var exchange2 = Secp521r1AgreementAdapter.Instance.GenerateKeyPair();

    var bouncy_private_key_agreement = PrivateKeyFactory.CreateKey(exchange.ExportPkcs8PrivateKey());
    var bouncy_public_key_agreement = PublicKeyFactory.CreateKey(exchange2.ExportSubjectPublicKeyInfo());


    var bouncy_secret = Secp521r1Adapter.CreateSecret((ECPrivateKeyParameters)bouncy_private_key_agreement, (ECPublicKeyParameters)bouncy_public_key_agreement);
    var windows_secret = exchange.DeriveRawSecretAgreement(exchange2.PublicKey);
    var match = Convert.ToBase64String(bouncy_secret) == Convert.ToBase64String(windows_secret);
}


static void Test384BouncyIntegration()
{
    var data = Utf8StringInterpolation.Utf8String.Format($"help");
    var windows = Secp384r1SigningAdapter.Instance.GenerateKeyPair();
    var hash = SHA256.HashData(data);

    var windows_signature = Secp384r1SigningAdapter.Instance.Sign(data, windows);

    var exchange = ECDiffieHellman.Create();
    exchange.ImportPkcs8PrivateKey(windows.ExportPkcs8PrivateKey(), out int _);

    var bouncy_private_key = PrivateKeyFactory.CreateKey(windows.ExportPkcs8PrivateKey());
    var bouncy_public_key = PublicKeyFactory.CreateKey(windows.ExportSubjectPublicKeyInfo());

    var bouncy_signature = Secp384r1Adapter.Instance.Sign(hash, bouncy_private_key);

    var windows_verify_bouncy = Secp384r1SigningAdapter.Instance.Verify(data, bouncy_signature, windows);
    var windows_verify_windows = Secp384r1SigningAdapter.Instance.Verify(data, windows_signature, windows);

    var bouncy_verify_bouncy = Secp384r1Adapter.Instance.Verify(hash, bouncy_signature, bouncy_public_key);
    var bouncy_verify_windows = Secp384r1Adapter.Instance.Verify(hash, windows_signature, bouncy_public_key);

    var exchange2 = Secp384r1AgreementAdapter.Instance.GenerateKeyPair();

    var bouncy_private_key_agreement = PrivateKeyFactory.CreateKey(exchange.ExportPkcs8PrivateKey());
    var bouncy_public_key_agreement = PublicKeyFactory.CreateKey(exchange2.ExportSubjectPublicKeyInfo());


    var bouncy_secret = Secp384r1Adapter.CreateSecret((ECPrivateKeyParameters)bouncy_private_key_agreement, (ECPublicKeyParameters)bouncy_public_key_agreement);
    var windows_secret = exchange.DeriveRawSecretAgreement(exchange2.PublicKey);
    var match = Convert.ToBase64String(bouncy_secret) == Convert.ToBase64String(windows_secret);
}

static void Test256BouncyIntegration()
{
    var data = Utf8StringInterpolation.Utf8String.Format($"help");
    var windows = Secp256r1SigningAdapter.Instance.GenerateKeyPair();
    var hash = SHA256.HashData(data);

    var windows_signed_hash = Secp256r1SigningAdapter.Instance.Sign(data, windows);

    var exchange = ECDiffieHellman.Create();
    exchange.ImportPkcs8PrivateKey(windows.ExportPkcs8PrivateKey(), out int _);

    var bouncy_private_key = PrivateKeyFactory.CreateKey(windows.ExportPkcs8PrivateKey());
    var bouncy_public_key = PublicKeyFactory.CreateKey(windows.ExportSubjectPublicKeyInfo());

    var bouncy_signature = Secp256r1Adapter.Instance.Sign(hash, bouncy_private_key);

    var windows_verify_bouncy = Secp256r1SigningAdapter.Instance.Verify(data, bouncy_signature, windows);
    var windows_verify_windows = Secp256r1SigningAdapter.Instance.Verify(data, windows_signed_hash, windows);

    var bouncy_verify_bouncy = Secp256r1Adapter.Instance.Verify(hash, bouncy_signature, bouncy_public_key);
    var bouncy_verify_windows = Secp256r1Adapter.Instance.Verify(hash, windows_signed_hash, bouncy_public_key);

    var exchange2 = Secp256r1AgreementAdapter.Instance.GenerateKeyPair();

    var bouncy_private_key_agreement = PrivateKeyFactory.CreateKey(exchange.ExportPkcs8PrivateKey());
    var bouncy_public_key_agreement = PublicKeyFactory.CreateKey(exchange2.ExportSubjectPublicKeyInfo());

    var bouncy_secret = Secp256r1Adapter.CreateSecret((ECPrivateKeyParameters)bouncy_private_key_agreement, (ECPublicKeyParameters)bouncy_public_key_agreement);
    var windows_secret = exchange.DeriveRawSecretAgreement(exchange2.PublicKey);
    var match = Convert.ToBase64String(bouncy_secret) == Convert.ToBase64String(windows_secret);
}


static void Test256()
{
    var alice = Secp256r1AgreementAdapter.Instance.GenerateKeyPair();
    var bob = Secp256r1AgreementAdapter.Instance.GenerateKeyPair();

    var secret = alice.DeriveRawSecretAgreement(bob.PublicKey);
    var secret2 = bob.DeriveRawSecretAgreement(alice.PublicKey);

    var alice2 = Secp256r1SigningAdapter.Instance.GenerateKeyPair();
    var bob2 = Secp256r1SigningAdapter.Instance.GenerateKeyPair();

    var signed = alice2.SignData(Utf8StringInterpolation.Utf8String.Format($"here"), HashAlgorithmName.SHA256);

    File.WriteAllBytes("AliceSecp256r1SigningAdapter.key", Secp256r1SigningAdapter.Instance.Export(alice2, true));
    File.WriteAllBytes("AliceSecp256r1SigningAdapter.cer", Secp256r1SigningAdapter.ExportX509PublicCertificate(alice2, "Genie").RawData);

    File.WriteAllBytes("AliceSecp256r1AgreementAdapter.key", Secp256r1AgreementAdapter.Instance.Export(alice, false));
    File.WriteAllBytes("AliceSecp256r1AgreementAdapter.cer", Secp256r1AgreementAdapter.Instance.ExportX509PublicCertificate(alice, "Genie").RawData);


    File.WriteAllBytes("BobSecp256r1SigningAdapter.key", Secp256r1SigningAdapter.Instance.Export(bob2, true));
    File.WriteAllBytes("BobSecp256r1SigningAdapter.cer", Secp256r1SigningAdapter.ExportX509PublicCertificate(bob2, "Genie").RawData);

    File.WriteAllBytes("BobSecp256r1AgreementAdapter.key", Secp256r1AgreementAdapter.Instance.Export(bob, false));
    File.WriteAllBytes("BobSecp256r1AgreementAdapter.cer", Secp256r1AgreementAdapter.Instance.ExportX509PublicCertificate(bob, "Genie").RawData);


    var exported = ECDsa.Create();
    exported.ImportSubjectPublicKeyInfo(alice2.ExportSubjectPublicKeyInfo(), out _);

    var validated = exported.VerifyData(Utf8StringInterpolation.Utf8String.Format($"here"), signed, HashAlgorithmName.SHA256);
}
static void Test384()
{
    var alice = Secp384r1AgreementAdapter.Instance.GenerateKeyPair();
    var bob = Secp384r1AgreementAdapter.Instance.GenerateKeyPair();

    var secret = alice.DeriveRawSecretAgreement(bob.PublicKey);
    var secret2 = bob.DeriveRawSecretAgreement(alice.PublicKey);

    var alice2 = Secp384r1SigningAdapter.Instance.GenerateKeyPair();
    var bob2 = Secp384r1SigningAdapter.Instance.GenerateKeyPair();

    var signed = alice2.SignData(Utf8StringInterpolation.Utf8String.Format($"here"), HashAlgorithmName.SHA256);

    File.WriteAllBytes("AliceSecp384r1SigningAdapter.key", Secp256r1SigningAdapter.Instance.Export(alice2, true));
    File.WriteAllBytes("AliceSecp384r1SigningAdapter.cer", Secp256r1SigningAdapter.ExportX509PublicCertificate(alice2, "Genie").RawData);

    File.WriteAllBytes("AliceSecp384r1AgreementAdapter.key", Secp256r1AgreementAdapter.Instance.Export(alice, false));
    File.WriteAllBytes("AliceSecp384r1AgreementAdapter.cer", Secp256r1AgreementAdapter.Instance.ExportX509PublicCertificate(alice, "Genie").RawData);


    File.WriteAllBytes("BobSecp384r1SigningAdapter.key", Secp256r1SigningAdapter.Instance.Export(bob2, true));
    File.WriteAllBytes("BobSecp384r1SigningAdapter.cer", Secp256r1SigningAdapter.ExportX509PublicCertificate(bob2, "Genie").RawData);

    File.WriteAllBytes("BobSecp384r1AgreementAdapter.key", Secp256r1AgreementAdapter.Instance.Export(bob, false));
    File.WriteAllBytes("BobSecp384r1AgreementAdapter.cer", Secp256r1AgreementAdapter.Instance.ExportX509PublicCertificate(bob, "Genie").RawData);

    var exported = ECDsa.Create();
    exported.ImportSubjectPublicKeyInfo(alice2.ExportSubjectPublicKeyInfo(), out _);

    var validated = exported.VerifyData(Utf8StringInterpolation.Utf8String.Format($"here"), signed, HashAlgorithmName.SHA256);
}

static void Test521()
{
    var alice = Secp521r1AgreementAdapter.Instance.GenerateKeyPair();
    var bob = Secp521r1AgreementAdapter.Instance.GenerateKeyPair();

    var secret = alice.DeriveRawSecretAgreement(bob.PublicKey);
    var secret2 = bob.DeriveRawSecretAgreement(alice.PublicKey);

    var alice2 = Secp521r1SigningAdapter.Instance.GenerateKeyPair();
    var bob2 = Secp521r1SigningAdapter.Instance.GenerateKeyPair();

    var signed = alice2.SignData(Utf8StringInterpolation.Utf8String.Format($"here"), HashAlgorithmName.SHA256);

    File.WriteAllBytes("AliceSecp521r1SigningAdapter.key", Secp256r1SigningAdapter.Instance.Export(alice2, true));
    File.WriteAllBytes("AliceSecp521r1SigningAdapter.cer", Secp256r1SigningAdapter.ExportX509PublicCertificate(alice2, "Genie").RawData);

    File.WriteAllBytes("AliceSecp521r1AgreementAdapter.key", Secp256r1AgreementAdapter.Instance.Export(alice, false));
    File.WriteAllBytes("AliceSecp521r1AgreementAdapter.cer", Secp256r1AgreementAdapter.Instance.ExportX509PublicCertificate(alice, "Genie").RawData);


    File.WriteAllBytes("BobSecp521r1SigningAdapter.key", Secp256r1SigningAdapter.Instance.Export(bob2, true));
    File.WriteAllBytes("BobSecp521r1SigningAdapter.cer", Secp256r1SigningAdapter.ExportX509PublicCertificate(bob2, "Genie").RawData);

    File.WriteAllBytes("BobSecp521r1AgreementAdapter.key", Secp256r1AgreementAdapter.Instance.Export(bob, false));
    File.WriteAllBytes("BobSecp521r1AgreementAdapter.cer", Secp256r1AgreementAdapter.Instance.ExportX509PublicCertificate(bob, "Genie").RawData);

    var exported = ECDsa.Create();
    exported.ImportSubjectPublicKeyInfo(alice2.ExportSubjectPublicKeyInfo(), out _);

    var validated = exported.VerifyData(Utf8StringInterpolation.Utf8String.Format($"here"), signed, HashAlgorithmName.SHA256);
}

void TestBitcoin()
{
    var alice = Secp256k1Adapter.Instance.GenerateKeyPair();
    var bob = Secp256k1Adapter.Instance.GenerateKeyPair();

    var alice_private = (ECPrivateKeyParameters)alice.Private;
    var alice_public = (ECPublicKeyParameters)alice.Public;

    var bob_private = (ECPrivateKeyParameters)bob.Private;
    var bob_public = (ECPublicKeyParameters)bob.Public;


    // Test the exchange
    var alice_secret2 = Secp256k1Adapter.CreateSecret(alice_private, bob_public);
    var bob_secret2 = Secp256k1Adapter.CreateSecret(bob_private, alice_public);

    File.WriteAllBytes("AliceSecp256k1Adapter.key", Secp256k1Adapter.Instance.Export(alice_private, true));
    File.WriteAllBytes("AliceSecp256k1Adapter.cer", Secp256k1Adapter.ExportX509PublicCertificate(alice, "Genie").RawData);

    File.WriteAllBytes("BobSecp256k1Adapter.key", Secp256k1Adapter.Instance.Export(bob_private, true));
    File.WriteAllBytes("BobSecp256k1Adapter.cer", Secp256k1Adapter.ExportX509PublicCertificate(bob, "Genie").RawData);

    var match = Convert.ToBase64String(alice_secret2) == Convert.ToBase64String(bob_secret2);


    // Test the signing
    var signed = Secp256k1Adapter.Instance.Sign(Utf8StringInterpolation.Utf8String.Format($"help"), alice_private);
    var signed_match = Secp256k1Adapter.Instance.Verify(Utf8StringInterpolation.Utf8String.Format($"help"), signed, alice_public);
}

static void TestRsa4096()
{
    var alice = Rsa4096Adapter.Instance.GenerateKeyPair();
    var bob = Rsa4096Adapter.Instance.GenerateKeyPair();

    var signed = alice.SignData(Utf8StringInterpolation.Utf8String.Format($"help"), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    var alice_private_exported = Rsa4096Adapter.Instance.Export(alice.ExportParameters(true), true);
    var alice_private_imported = Rsa4096Adapter.Import(new Genie.Common.Types.GeoCryptoKey { IsPrivate = true, X509 = alice_private_exported });

    var alice_public_exported = Rsa4096Adapter.ExportX509Certificate(alice, "Genie");
    var alice_public_imported = Rsa4096Adapter.Import(new Genie.Common.Types.GeoCryptoKey { IsPrivate = false, X509 = alice_public_exported.RawData });

    var bob_private_exported = Rsa4096Adapter.Instance.Export(alice.ExportParameters(true), true);
    var bob_public_exported = Rsa4096Adapter.ExportX509Certificate(alice, "Genie");


    File.WriteAllBytes("AliceRsa4096Adapter.key", alice_private_exported);
    File.WriteAllBytes("AliceRsa4096Adapter.cer", alice_public_exported.RawData);

    File.WriteAllBytes("BobRsa4096Adapter.key", bob_private_exported);
    File.WriteAllBytes("BobRsa4096Adapter.cer", bob_public_exported.RawData);



    var match = alice_public_imported.VerifyData(Utf8StringInterpolation.Utf8String.Format($"help"), signed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    var encrypted = alice_public_imported.Encrypt(Utf8StringInterpolation.Utf8String.Format($"help"), RSAEncryptionPadding.Pkcs1);
    var decrypted = alice.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);

    var match2 = System.Text.Encoding.UTF8.GetString(decrypted) == "help";
}

static void TestRsa2048()
{
    var alice = Rsa2048Adapter.Instance.GenerateKeyPair();
    var bob = Rsa2048Adapter.Instance.GenerateKeyPair();

    var signed = alice.SignData(Utf8StringInterpolation.Utf8String.Format($"help"), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    var alice_private_exported = Rsa2048Adapter.Instance.Export(alice.ExportParameters(true), true);
    var alice_private_imported = Rsa2048Adapter.Import(new Genie.Common.Types.GeoCryptoKey { IsPrivate = true, X509 = alice_private_exported });

    var alice_public_exported = Rsa2048Adapter.ExportX509Certificate(alice, "Genie");
    var alice_public_imported = Rsa2048Adapter.Import(new Genie.Common.Types.GeoCryptoKey { IsPrivate = false, X509 = alice_public_exported.RawData });

    var bob_private_exported = Rsa2048Adapter.Instance.Export(alice.ExportParameters(true), true);
    var bob_public_exported = Rsa2048Adapter.ExportX509Certificate(alice, "Genie");


    File.WriteAllBytes("AliceRsa2048Adapter.key", alice_private_exported);
    File.WriteAllBytes("AliceRsa2048Adapter.cer", alice_public_exported.RawData);

    File.WriteAllBytes("BobRsa2048Adapter.key", bob_private_exported);
    File.WriteAllBytes("BobRsa2048Adapter.cer", bob_public_exported.RawData);

    var match = alice_private_imported.VerifyData(Utf8StringInterpolation.Utf8String.Format($"help"), signed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    var encrypted = alice_private_imported.Encrypt(Utf8StringInterpolation.Utf8String.Format($"help"), RSAEncryptionPadding.Pkcs1);
    var decrypted = alice.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);

    var match2 = System.Text.Encoding.UTF8.GetString(decrypted) == "help";
}

static void TestRsa1024()
{
    var alice = Rsa1024Adapter.Instance.GenerateKeyPair();
    var bob = Rsa1024Adapter.Instance.GenerateKeyPair();

    var signed = alice.SignData(Utf8StringInterpolation.Utf8String.Format($"help"), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    var alice_private_exported = Rsa1024Adapter.Instance.Export(alice.ExportParameters(true), true);
    var alice_private_imported = Rsa1024Adapter.Import(new Genie.Common.Types.GeoCryptoKey { IsPrivate = true, X509 = alice_private_exported });

    var alice_public_exported = Rsa1024Adapter.ExportX509Certificate(alice, "Genie");
    var alice_public_imported = Rsa1024Adapter.Import(new Genie.Common.Types.GeoCryptoKey { IsPrivate = false, X509 = alice_public_exported.RawData });

    var bob_private_exported = Rsa1024Adapter.Instance.Export(alice.ExportParameters(true), true);
    var bob_public_exported = Rsa1024Adapter.ExportX509Certificate(alice, "Genie");


    File.WriteAllBytes("AliceRsa1024Adapter.key", alice_private_exported);
    File.WriteAllBytes("AliceRsa1024Adapter.cer", alice_public_exported.RawData);

    File.WriteAllBytes("BobRsa1024Adapter.key", bob_private_exported);
    File.WriteAllBytes("BobRsa1024Adapter.cer", bob_public_exported.RawData);

    var match = alice_private_imported.VerifyData(Utf8StringInterpolation.Utf8String.Format($"help"), signed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    var encrypted = alice_private_imported.Encrypt(Utf8StringInterpolation.Utf8String.Format($"help"), RSAEncryptionPadding.Pkcs1);
    var decrypted = alice.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);

    var match2 = System.Text.Encoding.UTF8.GetString(decrypted) == "help";
}



