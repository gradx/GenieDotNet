using BenchmarkDotNet.Attributes;
using Genie.Common.Crypto.Adapters.Curve25519;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Genie.Benchmarks.Benchmarks.Encryption
{
    public class CurveBenchmarks : EncryptionBenchmarkBase
    {
        private readonly byte[] ed25519_signed;
        private readonly byte[] ed448_signed;

        private readonly byte[] ed448_private_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\AliceEd448.key");
        private readonly byte[] ed448_public_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\AliceEd448.cer");
        private readonly AsymmetricKeyParameter ed448_private;
        private readonly AsymmetricKeyParameter ed448_public;

        private readonly byte[] ed25519_private_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\Ed25519SigningAdapter.key");
        private readonly byte[] ed25519_public_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\Ed25519SigningAdapter.cer");
        private readonly AsymmetricKeyParameter ed25519_private;
        private readonly AsymmetricKeyParameter ed25519_public;

        private readonly byte[] x25519_private_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\X25519Adapter.key");
        private readonly byte[] x25519_public_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\X25519Adapter.cer");
        private readonly AsymmetricCipherKeyPair x25519_alice;
        private readonly AsymmetricCipherKeyPair x25519_bob;

        private readonly byte[] x448_private_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\AliceX448.key");
        private readonly byte[] x448_public_bytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Curve25519\AliceX448.cer");
        private readonly AsymmetricCipherKeyPair x448_alice;
        private readonly AsymmetricCipherKeyPair x448_bob;

        private readonly byte[] x25519_alice_secret = new byte[32];
        private readonly byte[] x448_alice_secret = new byte[2456];

        public CurveBenchmarks()
        {

            // Ed25519
            ed25519_private = Ed25519Adapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = ed25519_private_bytes,
                IsPrivate = true
            });

            ed25519_public = Ed25519Adapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = ed25519_public_bytes,
                IsPrivate = false
            });

            ed25519_signed = Ed25519Adapter.Instance.Sign(hash, ed25519_private);
            var ed_result = Ed25519Adapter.Instance.Verify(hash, ed25519_signed, ed25519_public);

            // Ed448
            ed448_private = Ed448Adapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = ed448_private_bytes,
                KeyType = Common.Types.GeoCryptoKey.CryptoKeyType.Ed25519,
                IsPrivate = true
            });

            ed448_public = Ed448Adapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = ed448_public_bytes,
                KeyType = Common.Types.GeoCryptoKey.CryptoKeyType.Ed25519,
                IsPrivate = false
            });

            ed448_signed = Ed448Adapter.Instance.Sign(hash, ed448_private);
            var ed448_result = Ed448Adapter.Instance.Verify(hash, ed448_signed, ed448_public);




            // X25519
            x25519_alice = X25519Adapter.GenerateKeyPair();
            x25519_bob = X25519Adapter.GenerateKeyPair();
            var priv = (X25519PrivateKeyParameters)x25519_alice.Private;
            priv.GenerateSecret((X25519PublicKeyParameters)x25519_bob.Public, x25519_alice_secret, 0);

            // X448
            x448_alice = X448Adapter.GenerateKeyPair();
            x448_bob = X448Adapter.GenerateKeyPair();
            var priv2 = (X448PrivateKeyParameters)x448_alice.Private;
            priv2.GenerateSecret((X448PublicKeyParameters)x448_bob.Public, x448_alice_secret, 0);
        }


        #region Curve25519_Signing

        [Benchmark]
        public void Ed25519_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var signed = Ed25519Adapter.Instance.Sign(hash, ed25519_private);
            });
        }

        [Benchmark]
        public void Ed25519_Verification()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Ed25519Adapter.Instance.Verify(hash, ed25519_signed, ed25519_public);
            });
        }

        [Benchmark]
        public void Ed25519_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Ed25519Adapter.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void Ed25519_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Ed25519Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = ed25519_private_bytes });
            });
        }

        [Benchmark]
        public void Ed25519_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Ed25519Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = ed25519_public_bytes });
            });
        }


        [Benchmark]
        public void Ed448_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var signed = Ed448Adapter.Instance.Sign(hash, ed448_private);
            });
        }

        [Benchmark]
        public void Ed448_Verification()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Ed448Adapter.Instance.Verify(hash, ed448_signed, ed448_public);
            });
        }

        [Benchmark]
        public void Ed448_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Ed448Adapter.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void Ed448_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Ed448Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = ed448_private_bytes });
            });
        }

        [Benchmark]
        public void Ed448_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Ed448Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = ed448_public_bytes });
            });
        }

        #endregion

        #region Curve25519_Agreement

        [Benchmark]
        public void X25519_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                X25519Adapter.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void X25519_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                X25519Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = x25519_private_bytes });
            });
        }

        [Benchmark]
        public void X25519_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                X25519Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = x25519_public_bytes });
            });
        }


        [Benchmark]
        public void X25519_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                // Create a secret with Alice's private key and Bob's public Key
                byte[] secret = new byte[32];
                var priv = (X25519PrivateKeyParameters)x25519_alice.Private;
                priv.GenerateSecret((X25519PublicKeyParameters)x25519_bob.Public, secret, 0);
            });
        }

        [Benchmark]
        public void X25519_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                byte[] secret2 = new byte[32];
                var priv2 = (X25519PrivateKeyParameters)x25519_bob.Private;
                priv2.GenerateSecret((X25519PublicKeyParameters)x25519_alice.Public, secret2, 0);
                //var ismatch = Convert.ToBase64String(x25519_alice_secret) == Convert.ToBase64String(secret2);
            });
        }

        [Benchmark]
        public void X448_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                X448Adapter.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void X448_ImportKey_Private()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                X448Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = x448_private_bytes });
            });
        }

        [Benchmark]
        public void X448_ImportKey_Public()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                X448Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = x448_public_bytes });
            });
        }

        [Benchmark]
        public void X448_Exchange_Sender()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                // Create a secret with Alice's private key and Bob's public Key
                byte[] secret = new byte[2456];
                var priv = (X448PrivateKeyParameters)x448_alice.Private;
                priv.GenerateSecret((X448PublicKeyParameters)x448_bob.Public, secret, 0);
            });
        }

        [Benchmark]
        public void X448_Exchange_Receiver()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                byte[] secret2 = new byte[2456];
                var priv2 = (X448PrivateKeyParameters)x448_bob.Private;
                priv2.GenerateSecret((X448PublicKeyParameters)x448_alice.Public, secret2, 0);
                //var ismatch = Convert.ToBase64String(x448_alice_secret) == Convert.ToBase64String(secret2);
            });
        }

        #endregion
    }
}
