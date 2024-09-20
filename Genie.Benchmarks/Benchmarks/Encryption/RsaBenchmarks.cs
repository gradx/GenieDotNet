using BenchmarkDotNet.Attributes;
using Genie.Common.Crypto.Adapters.Rsa;
using System.Security.Cryptography;

namespace Genie.Benchmarks.Benchmarks.Encryption
{
    public class RsaBenchmarks : EncryptionBenchmarkBase
    {
        private readonly RSA rsa1024_alice;
        //private readonly RSA rsa1024_bob;

        private readonly RSA rsa2048_alice;
        //private readonly RSA rsa2048_bob;

        private readonly RSA rsa4096_alice;
        //private readonly RSA rsa4096_bob;

        private readonly byte[] rsa1024_signed;
        private readonly byte[] rsa2048_signed;
        private readonly byte[] rsa4096_signed;

        private readonly byte[] rsa1024_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Rsa\AliceRsa1024Adapter.key");
        private readonly byte[] rsa2048_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Rsa\AliceRsa2048Adapter.key");
        private readonly byte[] rsa4096_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Rsa\AliceRsa4096Adapter.key");

        public RsaBenchmarks()
        {
            rsa1024_alice = RsaBaseAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = rsa1024_key,
                IsPrivate = true
            })!;

            rsa1024_signed = rsa1024_alice.SignData(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            rsa2048_alice = RsaBaseAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = rsa2048_key,
                IsPrivate = true
            })!;

            rsa2048_signed = rsa2048_alice.SignData(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            rsa4096_alice = RsaBaseAdapter.Import(new Common.Types.GeoCryptoKey
            {
                X509 = rsa4096_key,
                IsPrivate = true
            })!;

            rsa4096_signed = rsa4096_alice.SignData(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        [Benchmark]
        public void Rsa1024_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Rsa1024Adapter.Instance.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void Rsa1024_ImportKey()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                RsaBaseAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = rsa1024_key });
            });
        }

        [Benchmark]
        public void Rsa1024_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Rsa1024Adapter.Instance.Sign(hash, rsa1024_alice.ExportParameters(true));
            });
        }

        [Benchmark]
        public void Rsa1024_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Rsa1024Adapter.Instance.Verify(hash, rsa1024_signed, rsa1024_alice.ExportParameters(false));
            });
        }

        [Benchmark]
        public void Rsa2048_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Rsa2048Adapter.Instance.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void Rsa2048_ImportKey()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                RsaBaseAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = rsa2048_key });
            });
        }

        [Benchmark]
        public void Rsa2048_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Rsa2048Adapter.Instance.Sign(hash, rsa2048_alice.ExportParameters(true));
            });

        }

        [Benchmark]
        public void Rsa2048_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Rsa2048Adapter.Instance.Verify(hash, rsa2048_signed, rsa2048_alice.ExportParameters(false));
            });
        }

        [Benchmark]
        public void Rsa4096_KeyGeneration()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Rsa4096Adapter.Instance.GenerateKeyPair();
            });
        }

        [Benchmark]
        public void Rsa4096_ImportKey()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                RsaBaseAdapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, X509 = rsa4096_key });
            });
        }

        [Benchmark]
        public void Rsa4096_Signing()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Rsa4096Adapter.Instance.Sign(hash, rsa4096_alice.ExportParameters(true));
            });
        }

        [Benchmark]
        public void Rsa4096_Verify()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var result = Rsa4096Adapter.Instance.Verify(hash, rsa4096_signed, rsa4096_alice.ExportParameters(false));
            });
        }
    }
}
