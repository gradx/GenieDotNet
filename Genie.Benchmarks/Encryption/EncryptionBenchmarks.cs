using Genie.Common.Crypto.Adapters.Curve25519;
using Genie.Common.Crypto.Adapters.Pqc;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using System.IO.Hashing;

namespace Genie.Benchmarks.Benchmarks.Encryption
{
    public class EncryptionBenchmarkBase
    {
        protected readonly byte[] hash;
        protected readonly int threads = 1;

        public EncryptionBenchmarkBase()
        {
            var cityhash = new XxHash64();
            cityhash.Append(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\Ed25519SigningAdapter.key"));
            hash = cityhash.GetCurrentHash();
        }
    }

    public class EncryptionBenchmarks
    {
        public void GetStats()
        {
            var what = new EncryptionBenchmarks();

            //Console.WriteLine($@"Kyber512 Encapsulation {kyber512_alice_secret.GetEncapsulation().Length}");
            //Console.WriteLine($@"Kyber768 Encapsulation {kyber768_alice_secret.GetEncapsulation().Length}");
            //Console.WriteLine($@"Kyber1024 Encapsulation {kyber1024_alice_secret.GetEncapsulation().Length}");

            //Console.WriteLine($@"Ed25519 Signing {Ed25519Adapter.Instance.Sign(hash, ed25519_private).Length}");
            //Console.WriteLine($@"Ed448 Signing {Ed448Adapter.Instance.Sign(hash, ed448_private).Length}");

            //Console.WriteLine($@"Dilithium 2 Signing {DilithiumAdapter.Instance.Sign(hash, dilithium2_private).Length}");
            //Console.WriteLine($@"Dilithium 3 Signing {DilithiumAdapter.Instance.Sign(hash, dilithium3_private).Length}");
            //Console.WriteLine($@"Dilithium 5 Signing {DilithiumAdapter.Instance.Sign(hash, dilithium5_private).Length}");

            var test1 = DilithiumAdapter.GenerateKeyPair(DilithiumParameters.Dilithium2);
            Console.WriteLine($@"Dilithium 2 Private: {DilithiumAdapter.Instance.Export(test1.Private, true).Length} ({((DilithiumPrivateKeyParameters)test1.Private).GetEncoded().Length}) Public: {DilithiumAdapter.Instance.Export(test1.Public, false).Length} ({((DilithiumPublicKeyParameters)test1.Public).GetEncoded().Length})");
            var test2 = DilithiumAdapter.GenerateKeyPair(DilithiumParameters.Dilithium3);
            Console.WriteLine($@"Dilithium 3 Private: {DilithiumAdapter.Instance.Export(test2.Private, true).Length} ({((DilithiumPrivateKeyParameters)test2.Private).GetEncoded().Length}) Public: {DilithiumAdapter.Instance.Export(test2.Public, false).Length} ({((DilithiumPublicKeyParameters)test2.Public).GetEncoded().Length})");
            var test3 = DilithiumAdapter.GenerateKeyPair(DilithiumParameters.Dilithium5);
            Console.WriteLine($@"Dilithium 5 Private: {DilithiumAdapter.Instance.Export(test3.Private, true).Length} ({((DilithiumPrivateKeyParameters)test3.Private).GetEncoded().Length}) Public: {DilithiumAdapter.Instance.Export(test3.Public, false).Length} ({((DilithiumPublicKeyParameters)test3.Public).GetEncoded().Length})");

            var test4 = KyberAdapter.GenerateKeyPair(KyberParameters.kyber512);
            Console.WriteLine($@"Kyber 512 Private: {KyberAdapter.Export(test4.Private).Length} ({((KyberPrivateKeyParameters)test4.Private).GetEncoded().Length}) Public: {KyberAdapter.Export(test4.Public).Length} ({((KyberPublicKeyParameters)test4.Public).GetEncoded().Length})");
            var test5 = KyberAdapter.GenerateKeyPair(KyberParameters.kyber768);
            Console.WriteLine($@"Kyber 768 Private: {KyberAdapter.Export(test5.Private).Length} ({((KyberPrivateKeyParameters)test5.Private).GetEncoded().Length}) Public: {KyberAdapter.Export(test5.Public).Length} ({((KyberPublicKeyParameters)test5.Public).GetEncoded().Length})");
            var test6 = KyberAdapter.GenerateKeyPair(KyberParameters.kyber1024);
            Console.WriteLine($@"Kyber 1024 Private: {KyberAdapter.Export(test6.Private).Length} ({((KyberPrivateKeyParameters)test6.Private).GetEncoded().Length}) Public: {KyberAdapter.Export(test6.Public).Length} ({((KyberPublicKeyParameters)test6.Public).GetEncoded().Length})");


            var test7 = Ed25519Adapter.GenerateKeyPair();
            Console.WriteLine($@"Ed25519 Private: {Ed25519Adapter.Instance.Export(test7.Private, true).Length} Public: {Ed25519Adapter.Instance.Export(test7.Public, false).Length}");
            var test8 = Ed448Adapter.GenerateKeyPair();
            Console.WriteLine($@"Ed448 Private: {Ed448Adapter.Instance.Export(test8.Private, true).Length} Public: {Ed448Adapter.Instance.Export(test8.Public, false).Length}");

            var test9 = X25519Adapter.GenerateKeyPair();
            Console.WriteLine($@"X25519 Private: {X25519Adapter.Export(test9.Private).Length} Public: {X25519Adapter.Export(test9.Public).Length}");
            var test10 = X448Adapter.GenerateKeyPair();
            Console.WriteLine($@"X448 Private: {X448Adapter.Export(test10.Private).Length} Public: {X448Adapter.Export(test10.Public).Length}");
        }
    }
}
