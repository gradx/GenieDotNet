using BenchmarkDotNet.Attributes;
using Genie.Common.Crypto.Adapters.Pqc;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;

namespace Genie.Benchmarks.Benchmarks
{
    public class ModuleLatticeBenchmarks : EncryptionBenchmarkBase
    {
        private readonly byte[] dilithium5_signed;
        private readonly byte[] dilithium3_signed;
        private readonly byte[] dilithium2_signed;

        private readonly AsymmetricKeyParameter dilithium5_private;
        private readonly AsymmetricKeyParameter dilithium5_public;

        private readonly AsymmetricKeyParameter dilithium3_private;
        private readonly AsymmetricKeyParameter dilithium3_public;

        private readonly AsymmetricKeyParameter dilithium2_private;
        private readonly AsymmetricKeyParameter dilithium2_public;

        private readonly AsymmetricCipherKeyPair kyber512_alice;
        private readonly AsymmetricCipherKeyPair kyber512_bob;

        private readonly AsymmetricCipherKeyPair kyber768_alice;
        private readonly AsymmetricCipherKeyPair kyber768_bob;

        private readonly AsymmetricCipherKeyPair kyber1024_alice;
        private readonly AsymmetricCipherKeyPair kyber1024_bob;

        private readonly ISecretWithEncapsulation kyber512_alice_secret;
        private readonly ISecretWithEncapsulation kyber768_alice_secret;
        private readonly ISecretWithEncapsulation kyber1024_alice_secret;

        private readonly byte[] dilithium3_private_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\ModuleLattice\AliceDilithium3.key");
        private readonly byte[] dilithium3_public_cer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\ModuleLattice\AliceDilithium3.cer");
        private readonly byte[] dilithium5_private_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\ModuleLattice\AliceDilithium5.key");
        private readonly byte[] dilithium5_public_cer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\ModuleLattice\AliceDilithium5.cer");
        private readonly byte[] dilithium2_private_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\ModuleLattice\AliceDilithium2.key");
        private readonly byte[] dilithium2_public_cer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\ModuleLattice\AliceDilithium2.cer");

        private readonly byte[] kyber512_private_key;
        private readonly byte[] kyber512_public_cer;
        private readonly byte[] kyber768_private_key;
        private readonly byte[] kyber768_public_cer;
        private readonly byte[] kyber1024_private_key;
        private readonly byte[] kyber1024_public_cer;

        public ModuleLatticeBenchmarks() 
        {

            // Dilthium
            dilithium3_private = DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = dilithium3_private_key,
                IsPrivate = true
            });

            dilithium3_public = DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = dilithium3_public_cer,
                IsPrivate = false
            });

            dilithium3_signed = DilithiumAdapter.Instance.Sign(hash, dilithium3_private);
            var dil3_result = DilithiumAdapter.Instance.Verify(hash, dilithium3_signed, dilithium3_public);

            dilithium5_private = DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = dilithium5_private_key,
                IsPrivate = true
            });

            dilithium5_public = DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = dilithium5_public_cer,
                IsPrivate = false
            });

            dilithium5_signed = DilithiumAdapter.Instance.Sign(hash, dilithium5_private);
            var dil5_result = DilithiumAdapter.Instance.Verify(hash, dilithium5_signed, dilithium5_public);

            dilithium2_private = DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = dilithium2_private_key,
                IsPrivate = true
            });

            dilithium2_public = DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = dilithium2_public_cer,
                IsPrivate = false
            });

            dilithium2_signed = DilithiumAdapter.Instance.Sign(hash, dilithium2_private);
            var dil2_result = DilithiumAdapter.Instance.Verify(hash, dilithium2_signed, dilithium2_public);

            // Kyber
            kyber512_alice = KyberAdapter.GenerateKeyPair();
            kyber512_bob = KyberAdapter.GenerateKeyPair();

            kyber768_alice = KyberAdapter.GenerateKeyPair(KyberParameters.kyber768);
            kyber768_bob = KyberAdapter.GenerateKeyPair(KyberParameters.kyber768);

            kyber1024_alice = KyberAdapter.GenerateKeyPair(KyberParameters.kyber1024);
            kyber1024_bob = KyberAdapter.GenerateKeyPair(KyberParameters.kyber1024);

            kyber512_alice_secret = KyberAdapter.GenerateSecret((KyberPublicKeyParameters)kyber512_bob.Public);
            kyber768_alice_secret = KyberAdapter.GenerateSecret((KyberPublicKeyParameters)kyber768_bob.Public);
            kyber1024_alice_secret = KyberAdapter.GenerateSecret((KyberPublicKeyParameters)kyber1024_bob.Public);

            kyber512_private_key = KyberAdapter.Export(kyber512_alice.Private);
            kyber512_public_cer = KyberAdapter.Export(kyber512_alice.Public);

            kyber768_private_key = KyberAdapter.Export(kyber768_alice.Private);
            kyber768_public_cer = KyberAdapter.Export(kyber768_alice.Public);

            kyber1024_private_key = KyberAdapter.Export(kyber1024_alice.Private);
            kyber1024_public_cer = KyberAdapter.Export(kyber1024_alice.Public);
        }

        [Benchmark]
        public void Dilithium2_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var signed = DilithiumAdapter.Instance.Sign(hash, dilithium2_private);
            });
        }

        [Benchmark]
        public void Dilithium2_Verification()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = DilithiumAdapter.Instance.Verify(hash, dilithium2_signed, dilithium2_public);
            });
        }

        [Benchmark]
        public void Dilithium2_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.GenerateKeyPair(DilithiumParameters.Dilithium2);
            });
        }

        [Benchmark]
        public void Dilithium2_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = dilithium2_private_key });
            });
        }

        [Benchmark]
        public void Dilithium2_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = dilithium2_public_cer });
            });
        }



        [Benchmark]
        public void Dilithium3_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var signed = DilithiumAdapter.Instance.Sign(hash, dilithium3_private);
            });
        }

        [Benchmark]
        public void Dilithium3_Verification()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = DilithiumAdapter.Instance.Verify(hash, dilithium3_signed, dilithium3_public);
            });
        }

        [Benchmark]
        public void Dilithium3_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void Dilithium3_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = dilithium3_private_key });
            });
        }

        [Benchmark]
        public void Dilithium3_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = dilithium3_public_cer });
            });
        }

        [Benchmark]
        public void Dilithium5_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var signed = DilithiumAdapter.Instance.Sign(hash, dilithium5_private);
            });
        }

        [Benchmark]
        public void Dilithium5_Verification()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = DilithiumAdapter.Instance.Verify(hash, dilithium5_signed, dilithium5_public);
            });
        }

        [Benchmark]
        public void Dilithium5_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.GenerateKeyPair(DilithiumParameters.Dilithium5);
            });
        }

        [Benchmark]
        public void Dilithium5_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = dilithium5_private_key });
            });
        }

        [Benchmark]
        public void Dilithium5_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = dilithium5_public_cer });
            });
        }

        [Benchmark]
        public void Kyber512_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Kyber_KeyGeneration");
                mutex.WaitOne();

                KyberAdapter.GenerateKeyPair();
                mutex.ReleaseMutex();
            });
        }

        [Benchmark]
        public void Kyber512_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                KyberAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = kyber512_private_key });
            });
        }

        [Benchmark]
        public void Kyber512_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                KyberAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = kyber512_public_cer });
            });
        }

        [Benchmark]
        public void Kyber512_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Oops");
                mutex.WaitOne();
                // Create a secret with Alice's private key and Bob's public Key
                var priv = (KyberPrivateKeyParameters)kyber512_alice.Private;
                var secret = KyberAdapter.GenerateSecret((KyberPublicKeyParameters)kyber512_bob.Public);

                mutex.ReleaseMutex();
            });
        }

        [Benchmark]
        public void Kyber512_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Oops");
                mutex.WaitOne();

                var kyber = new KyberKemExtractor((KyberPrivateKeyParameters)kyber512_bob.Private);
                var result = kyber.ExtractSecret(kyber512_alice_secret.GetEncapsulation());
                mutex.ReleaseMutex();

                //var ismatch = Convert.ToBase64String(kyber512_alice_secret.GetSecret()) == Convert.ToBase64String(result);
            });
        }


        [Benchmark]
        public void Kyber768_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Kyber_KeyGeneration");
                mutex.WaitOne();

                KyberAdapter.GenerateKeyPair(KyberParameters.kyber768);
                mutex.ReleaseMutex();
            });
        }

        [Benchmark]
        public void Kyber768_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                KyberAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = kyber768_private_key });
            });
        }

        [Benchmark]
        public void Kyber768_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                KyberAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = kyber768_public_cer });
            });
        }

        [Benchmark]
        public void Kyber768_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Oops");
                mutex.WaitOne();
                // Create a secret with Alice's private key and Bob's public Key
                var priv = (KyberPrivateKeyParameters)kyber768_alice.Private;
                var secret = KyberAdapter.GenerateSecret((KyberPublicKeyParameters)kyber768_bob.Public);
                mutex.ReleaseMutex();
            });
        }

        [Benchmark]
        public void Kyber768_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Oops");
                mutex.WaitOne();

                var kyber = new KyberKemExtractor((KyberPrivateKeyParameters)kyber768_bob.Private);
                var result = kyber.ExtractSecret(kyber768_alice_secret.GetEncapsulation());
                mutex.ReleaseMutex();

                //var ismatch = Convert.ToBase64String(kyber768_alice_secret.GetSecret()) == Convert.ToBase64String(result);
            });
        }

        [Benchmark]
        public void Kyber1024_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Kyber_KeyGeneration");
                mutex.WaitOne();

                KyberAdapter.GenerateKeyPair(KyberParameters.kyber1024);
                mutex.ReleaseMutex();
            });
        }

        [Benchmark]
        public void Kyber1024_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                KyberAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = kyber1024_private_key });
            });
        }

        [Benchmark]
        public void Kyber1024_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                KyberAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = kyber1024_public_cer });
            });
        }

        [Benchmark]
        public void Kyber1024_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Oops");
                mutex.WaitOne();
                // Create a secret with Alice's private key and Bob's public Key
                var priv = (KyberPrivateKeyParameters)kyber1024_alice.Private;
                var secret = KyberAdapter.GenerateSecret((KyberPublicKeyParameters)kyber1024_bob.Public);
                mutex.ReleaseMutex();
            });
        }

        [Benchmark]
        public void Kyber1024_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Mutex mutex = new Mutex(false, "Oops");
                mutex.WaitOne();

                var kyber = new KyberKemExtractor((KyberPrivateKeyParameters)kyber1024_bob.Private);
                var result = kyber.ExtractSecret(kyber1024_alice_secret.GetEncapsulation());
                mutex.ReleaseMutex();

                //var ismatch = Convert.ToBase64String(kyber1024_alice_secret.GetSecret()) == Convert.ToBase64String(result);
            });
        }
    }
}
