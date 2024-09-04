using BenchmarkDotNet.Attributes;
using Org.BouncyCastle.Ocsp;
using System.Net;
using System.Security.Cryptography.X509Certificates;
namespace Genie.Benchmarks
{
    public class PqcNetworkBenchmarks
    {
        private const string c_URL = "https://luxur.ai:5003";
        private readonly string c_CERTIFICATE = AppDomain.CurrentDomain.BaseDirectory + @"luxePod.pfx";
        private readonly byte[] x25519_ed25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\x25519_ed25519.req");
        private readonly byte[] x25519_dilithium = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\x25519_dilithium.req");
        private readonly byte[] kyber_ed25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\kyber_ed25519.req");
        private readonly byte[] kyber_dilithium = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"EncryptionRequests\kyber_dilithium.req");

        private readonly int threads = 32;

        [Benchmark]
        public void Kyber_Ed25519()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var content = new MultipartFormDataContent();
                var ms = new MemoryStream(kyber_ed25519);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", content).GetAwaiter().GetResult();
            });
        }

        [Benchmark]
        public void Kyber_Dilithium()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var content = new MultipartFormDataContent();
                var ms = new MemoryStream(kyber_dilithium);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", content).GetAwaiter().GetResult();
            });
        }

        [Benchmark]
        public void X25519_Ed25519()
        {

            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var content = new MultipartFormDataContent();
                var ms = new MemoryStream(x25519_ed25519);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", content).GetAwaiter().GetResult();
            });
        }

        [Benchmark]
        public void X25519_Dilithium()
        {

            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var client = CreateHttpClient(c_CERTIFICATE);

                var content = new MultipartFormDataContent();
                var ms = new MemoryStream(x25519_dilithium);
                var pr = new StreamContent(ms);
                var resp = client.PostAsync("https://localhost:5003/encryption", content).GetAwaiter().GetResult();
            });
        }



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
