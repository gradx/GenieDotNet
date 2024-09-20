using BenchmarkDotNet.Attributes;
using Genie.Common.Crypto.Adapters.Bouncy;
using Genie.Common.Crypto.Adapters.Nist;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;

namespace Genie.Benchmarks.Benchmarks.Encryption
{
    public class SecpBenchmarks : EncryptionBenchmarkBase
    {
        private readonly AsymmetricCipherKeyPair secp256k1_b_alice;
        private readonly AsymmetricCipherKeyPair secp256k1_b_bob;

        private readonly AsymmetricCipherKeyPair secp256r1_b_alice;
        private readonly AsymmetricCipherKeyPair secp256r1_b_bob;

        private readonly AsymmetricCipherKeyPair secp384r1_b_alice;
        private readonly AsymmetricCipherKeyPair secp384r1_b_bob;

        private readonly AsymmetricCipherKeyPair secp521r1_b_alice;
        private readonly AsymmetricCipherKeyPair secp521r1_b_bob;

        private readonly ECDsa secp256r1_signing_private;
        private readonly ECDsa secp256r1_signing_public;

        private readonly ECDsa secp384r1_signing_private;
        private readonly ECDsa secp384r1_signing_public;

        private readonly ECDsa secp521r1_signing_private;
        private readonly ECDsa secp521r1_signing_public;

        private readonly byte[] secp256k1_signed;
        private readonly byte[] secp256r1_signed;
        private readonly byte[] bouncy_secp256r1_signed;
        private readonly byte[] secp384r1_signed;
        private readonly byte[] bouncy_secp384r1_signed;
        private readonly byte[] secp521r1_signed;
        private readonly byte[] bouncy_secp521r1_signed;

        private readonly ECDiffieHellman secp256r1_alice;
        private readonly ECDiffieHellman secp256r1_bob;

        private readonly ECDiffieHellman secp384r1_alice;
        private readonly ECDiffieHellman secp384r1_bob;

        private readonly ECDiffieHellman secp521r1_alice;
        private readonly ECDiffieHellman secp521r1_bob;

        private readonly byte[] secp256r1_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp256r1SigningAdapter.key");
        private readonly byte[] secp256r1_cer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp256r1SigningAdapter.cer");
        private readonly byte[] secp384r1_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp384r1SigningAdapter.key");
        private readonly byte[] secp384r1_cer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp384r1SigningAdapter.cer");
        private readonly byte[] secp521r1_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp521r1SigningAdapter.key");
        private readonly byte[] secp521r1_cer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp521r1SigningAdapter.cer");

        private readonly byte[] secp256k1_signing_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp256k1Adapter.key");
        private readonly byte[] secp256k1_signing_cer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Secp\AliceSecp256k1Adapter.cer");

        //private readonly byte[] secp256k1_agreement_key;
        //private readonly byte[] secp256k1_agreement_cer;

        private readonly byte[] bouncy_256r1_public;
        private readonly byte[] bouncy_384r1_public;
        private readonly byte[] bouncy_521r1_public;

        public SecpBenchmarks()
        {

            // secp256r1
            secp256r1_signing_private = SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = secp256r1_key,
                IsPrivate = true
            })!;

            secp256r1_signing_public = SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = secp256r1_cer,
                IsPrivate = false
            })!;

            bouncy_256r1_public = secp256r1_signing_public.ExportSubjectPublicKeyInfo();

            secp256r1_signed = Secp256r1SigningAdapter.Instance.Sign(hash, secp256r1_signing_private);
            var secp256r1_result = Secp256r1SigningAdapter.Instance.Verify(hash, secp256r1_signed, secp256r1_signing_public);


            secp256r1_alice = Secp256r1AgreementAdapter.Instance.GenerateKeyPair();
            secp256r1_bob = Secp256r1AgreementAdapter.Instance.GenerateKeyPair();


            // secp384r1
            secp384r1_signing_private = SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = secp384r1_key,
                IsPrivate = true
            })!;

            secp384r1_signing_public = SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = secp384r1_cer,
                IsPrivate = false
            })!;

            bouncy_384r1_public = secp384r1_signing_public.ExportSubjectPublicKeyInfo();

            secp384r1_signed = Secp384r1SigningAdapter.Instance.Sign(hash, secp384r1_signing_private);
            var secp384r1_result = Secp384r1SigningAdapter.Instance.Verify(hash, secp384r1_signed, secp384r1_signing_public);

            secp384r1_alice = Secp384r1AgreementAdapter.Instance.GenerateKeyPair();
            secp384r1_bob = Secp384r1AgreementAdapter.Instance.GenerateKeyPair();


            // secp521r1
            secp521r1_signing_private = SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = secp521r1_key,
                IsPrivate = true
            })!;

            secp521r1_signing_public = SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = secp521r1_cer,
                IsPrivate = false
            })!;

            bouncy_521r1_public = secp521r1_signing_public.ExportSubjectPublicKeyInfo();

            secp521r1_signed = Secp521r1SigningAdapter.Instance.Sign(hash, secp521r1_signing_private);
            var secp521r1_result = Secp521r1SigningAdapter.Instance.Verify(hash, secp521r1_signed, secp521r1_signing_public);

            secp521r1_alice = Secp521r1AgreementAdapter.Instance.GenerateKeyPair();
            secp521r1_bob = Secp521r1AgreementAdapter.Instance.GenerateKeyPair();

            secp256r1_b_alice = Secp256r1Adapter.Instance.GenerateKeyPair();
            secp256r1_b_bob = Secp256r1Adapter.Instance.GenerateKeyPair();

            secp384r1_b_alice = Secp384r1Adapter.Instance.GenerateKeyPair();
            secp384r1_b_bob = Secp384r1Adapter.Instance.GenerateKeyPair();

            secp521r1_b_alice = Secp521r1Adapter.Instance.GenerateKeyPair();
            secp521r1_b_bob = Secp521r1Adapter.Instance.GenerateKeyPair();

            secp256k1_b_alice = Secp256k1Adapter.Instance.GenerateKeyPair();
            secp256k1_b_bob = Secp256k1Adapter.Instance.GenerateKeyPair();

            bouncy_secp256r1_signed = Secp256r1Adapter.Instance.Sign(hash, secp256r1_b_alice.Private);
            bouncy_secp384r1_signed = Secp384r1Adapter.Instance.Sign(hash, secp384r1_b_alice.Private);
            bouncy_secp521r1_signed = Secp521r1Adapter.Instance.Sign(hash, secp521r1_b_alice.Private);

            secp256k1_signed = Secp256k1Adapter.Instance.Sign(hash, secp256k1_b_alice.Private);

            // secp256k1
        }

        #region Secp
        //[Benchmark]
        public void Bouncy_Secp256k1_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256k1Adapter.Instance.GenerateKeyPair();
            });

        }

        //[Benchmark]
        public void Bouncy_Secp256k1_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256k1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp256k1_signing_key });
            });

        }

        //[Benchmark]
        public void Bouncy_Secp256k1_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256k1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = secp256k1_signing_cer });
            });

        }


        //[Benchmark]
        public void Bouncy_Secp256k1_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp256k1Adapter.Instance.Sign(hash, secp256k1_b_alice.Private);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp256k1_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp256k1Adapter.Instance.Verify(hash, secp256k1_signed, secp256k1_b_alice.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp256k1_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp256k1_b_alice.Private, (ECPublicKeyParameters)secp256k1_b_bob.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp256k1_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp256k1_b_bob.Private, (ECPublicKeyParameters)secp256k1_b_alice.Public);
            });
        }

        // Bouncy Secp256r1

        //[Benchmark]
        public void Bouncy_Secp256r1_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256r1Adapter.Instance.GenerateKeyPair();
            });
        }


        //[Benchmark]
        public void Bouncy_Secp256r1_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp256r1_key });
            });

        }

        //[Benchmark]
        public void Bouncy_Secp256r1_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = bouncy_256r1_public });
            });

        }

        //[Benchmark]
        public void Bouncy_Secp256r1_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp256r1Adapter.Instance.Sign(hash, secp256r1_b_alice.Private);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp256r1_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp256r1Adapter.Instance.Verify(hash, bouncy_secp256r1_signed, secp256r1_b_alice.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp256r1_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp256r1_b_alice.Private, (ECPublicKeyParameters)secp256r1_b_bob.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp256r1_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp256r1_b_bob.Private, (ECPublicKeyParameters)secp256r1_b_alice.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp384r1_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256r1Adapter.Instance.GenerateKeyPair();
            });
        }


        //[Benchmark]
        public void Bouncy_Secp384r1_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp384r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp384r1_key });
            });

        }

        //[Benchmark]
        public void Bouncy_Secp384r1_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp384r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = bouncy_384r1_public });
            });

        }


        //[Benchmark]
        public void Bouncy_Secp384r1_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp384r1Adapter.Instance.Sign(hash, secp384r1_b_alice.Private);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp384r1_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp384r1Adapter.Instance.Verify(hash, secp384r1_signed, secp384r1_b_alice.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp384r1_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp384r1_b_alice.Private, (ECPublicKeyParameters)secp384r1_b_bob.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp384r1_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp384r1_b_bob.Private, (ECPublicKeyParameters)secp384r1_b_alice.Public);
            });
        }

        [Benchmark]
        public void Bouncy_Secp521r1_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp521r1Adapter.Instance.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void Bouncy_Secp521r1_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp521r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp521r1_key });
            });

        }

        [Benchmark]
        public void Bouncy_Secp521r1_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp521r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = bouncy_521r1_public });
            });

        }


        //[Benchmark]
        public void Bouncy_Secp521r1_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp521r1Adapter.Instance.Sign(hash, secp521r1_b_alice.Private);
            });
        }

        //[Benchmark]
        public void Bouncy_Secp521r1_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp521r1Adapter.Instance.Verify(hash, bouncy_secp521r1_signed, secp521r1_b_alice.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Sec521r1_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp521r1_b_alice.Private, (ECPublicKeyParameters)secp521r1_b_bob.Public);
            });
        }

        //[Benchmark]
        public void Bouncy_Sec521r1_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = SecpBaseAdapter.CreateSecret((ECPrivateKeyParameters)secp521r1_b_bob.Private, (ECPublicKeyParameters)secp521r1_b_alice.Public);
            });
        }

        // Native Secp256r1

        //[Benchmark]
        public void Secp256r1_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256r1AgreementAdapter.Instance.GenerateKeyPair();
            });
        }


        //[Benchmark]
        public void Secp256r1_ImportSigningKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp256r1_key });
            });

        }

        //[Benchmark]
        public void Secp256r1_ImportSigningKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {

                SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = secp256r1_cer });
            });

        }

        //[Benchmark]
        public void Secp256r1_ImportAgreementKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp256r1AgreementAdapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp256r1_key });
            });

        }

        //[Benchmark]
        public void Secp256r1_ImportAgreementKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {

                Secp256r1AgreementAdapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = secp256r1_cer });
            });

        }

        //[Benchmark]
        public void Secp256r1_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = secp256r1_alice.DeriveRawSecretAgreement(secp256r1_bob.PublicKey);
            });
        }

        //[Benchmark]
        public void Secp256r1_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = secp256r1_bob.DeriveRawSecretAgreement(secp256r1_alice.PublicKey);
            });
        }

        //[Benchmark]
        public void Secp256r1_Sign_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp256r1SigningAdapter.Instance.Sign(hash, secp256r1_signing_private);
            });
        }

        //[Benchmark]
        public void Secp256r1_Sign_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp256r1SigningAdapter.Instance.Verify(hash, secp256r1_signed, secp256r1_signing_public);
            });
        }


        //[Benchmark]
        public void Secp384r1_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp384r1AgreementAdapter.Instance.GenerateKeyPair();
            });
        }

        //[Benchmark]
        public void Secp384r1_ImportSigningKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp384r1_key });
            });

        }

        //[Benchmark]
        public void Secp384r1_ImportSigningKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = secp384r1_cer });
            });

        }

        //[Benchmark]
        public void Secp384r1_ImportAgreementKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp384r1AgreementAdapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp384r1_key });
            });

        }

        [Benchmark]
        public void Secp384r1_ImportAgreementKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {

                Secp384r1AgreementAdapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = secp384r1_cer });
            });

        }

        [Benchmark]
        public void Secp384r1_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = secp384r1_alice.DeriveRawSecretAgreement(secp384r1_bob.PublicKey);
            });
        }

        [Benchmark]
        public void Secp384r1_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = secp384r1_bob.DeriveRawSecretAgreement(secp384r1_alice.PublicKey);
            });
        }

        //[Benchmark]
        public void Secp384r1_Sign_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp384r1SigningAdapter.Instance.Sign(hash, secp384r1_signing_private);
            });
        }

        //[Benchmark]
        public void Secp384r1_Sign_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp384r1SigningAdapter.Instance.Verify(hash, secp384r1_signed, secp384r1_signing_public);
            });
        }

        //[Benchmark]
        public void Secp521r1_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp521r1AgreementAdapter.Instance.GenerateKeyPair();
            });
        }

        //[Benchmark]
        public void Secp521r1_ImportSigningKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp521r1_key });
            });

        }

        //[Benchmark]
        public void Secp521r1_ImportSigningKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                SecpBaseSigningAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = secp521r1_cer });
            });

        }


        //[Benchmark]
        public void Secp521r1_ImportAgreementKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Secp521r1AgreementAdapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = secp521r1_key });
            });

        }

        //[Benchmark]
        public void Secp521r1_ImportAgreementKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {

                Secp521r1AgreementAdapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = secp521r1_cer });
            });

        }

        //[Benchmark]
        public void Secp521r1_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = secp521r1_alice.DeriveRawSecretAgreement(secp521r1_bob.PublicKey);
            });
        }

        //[Benchmark]
        public void Secp521r1_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = secp521r1_bob.DeriveRawSecretAgreement(secp521r1_alice.PublicKey);
            });
        }

        //[Benchmark]
        public void Secp521r1_Sign_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp521r1SigningAdapter.Instance.Sign(hash, secp521r1_signing_private);
            });
        }

        //[Benchmark]
        public void Secp521r1_Sign_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Secp521r1SigningAdapter.Instance.Verify(hash, secp521r1_signed, secp521r1_signing_public);
            });
        }

        #endregion

    }
}
