using BenchmarkDotNet.Attributes;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Genie.Benchmarks
{
    public class PqcNetworkBenchmarks
    {
        private const string c_URL = "https://luxur.ai:5003";
        private readonly string c_CERTIFICATE = AppDomain.CurrentDomain.BaseDirectory + @"luxePod.pfx";
        private readonly byte[] x25519_ed25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\x25519_ed25519.req");
        //private readonly byte[] x25519_dilithium = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\x25519_dilithium3.req");
        private readonly byte[] kyber_ed25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\kyber512_ed25519.req");
        private readonly byte[] kyber_dilithium = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\kyber512_dilithium3.req");

        private readonly byte[] secp256k1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\Secp256K1_Secp256K1.req");
        private readonly byte[] secp256r1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\Secp256R1_Secp256R1.req");
        private readonly byte[] secp384r1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\Secp384R1_Secp384R1.req");
        private readonly byte[] secp521r1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\Secp521R1_Secp521R1.req");
        private readonly byte[] ed448 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\X448_Ed448.req");

        private readonly int threads = 1;

        public void Ed448()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(ed448);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }


        public void Secp256k1()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(secp256k1);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }

        public void Secp256r1()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(secp256r1);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }

        public void Secp384r1()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(secp384r1);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }

        public void Secp521r1()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(secp521r1);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }


        [Benchmark]
        public void Kyber_Ed25519()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(kyber_ed25519);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }

        [Benchmark]
        public void Kyber_Dilithium()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(kyber_dilithium);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }

        [Benchmark]
        public void X25519_Ed25519()
        {

            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var ms = new MemoryStream(x25519_ed25519);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
            });
        }

        //[Benchmark]
        //public void X25519_Dilithium()
        //{

        //    Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
        //    {
        //        var client = CreateHttpClient(c_CERTIFICATE);

        //        var ms = new MemoryStream(x25519_dilithium);
        //        var pr = new StreamContent(ms);
        //        var resp = client.PostAsync("https://localhost:5003/encryption", pr).GetAwaiter().GetResult();
        //    });
        //}



        private static HttpClient CreateHttpClient(string certificate, string password = "")
        {
            var handler = new HttpClientHandler();
            var cert = new X509Certificate2(certificate, password);
            handler.ClientCertificates.Add(cert);
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return new HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
            };
        }

    }
}
