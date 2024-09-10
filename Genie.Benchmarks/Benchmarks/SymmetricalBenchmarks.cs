using BenchmarkDotNet.Attributes;
using Genie.Common.Crypto.Adapters.Curve25519;
using Genie.Common.Crypto.Adapters.Nist;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;
using Utf8StringInterpolation;

namespace Genie.Benchmarks.Benchmarks
{
    public class SymmetricalBenchmarks : EncryptionBenchmarkBase
    {
        readonly byte[] data = Utf8String.Format($"Genie in a Bottle");

        readonly AsymmetricCipherKeyPair alice;
        readonly AsymmetricCipherKeyPair bob;
        readonly byte[] nonce = Utf8String.Format($"{RandomString(new StringBuilder(), 12)}");
        readonly byte[] hkdfKey;
        readonly byte[] chachaPolyKey;
        readonly KeyParameter keyParam;
        readonly KeyParameter keyParamChaChaPoly;


        public static string RandomString(StringBuilder sb, int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@";

            while (length-- > 0)
            {
                var rng = RandomNumberGenerator.GetBytes(sizeof(uint));
                uint num = BitConverter.ToUInt32(rng, 0);
                sb.Append(valid[(int)(num % (uint)valid.Length)]);
            }

            var result = sb.ToString();
            return result;
        }

        public SymmetricalBenchmarks()
        {
            alice = X25519Adapter.GenerateKeyPair();
            bob = X25519Adapter.GenerateKeyPair();
            var secret = X25519Adapter.GenerateSecret((X25519PrivateKeyParameters)alice.Private, (X25519PublicKeyParameters)bob.Public);

            var hkdf_salt = Utf8String.Format($"{RandomString(new StringBuilder(), 16)}");
            var nonce = Utf8String.Format($"{RandomString(new StringBuilder(), 12)}");

            // Create an HDKF key
            var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"Push It - Salt & Pepper"));
            hkdfKey = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, hkdf_salt);

            chachaPolyKey = HKDF.Expand(HashAlgorithmName.SHA256, extract, 32, hkdf_salt);

            keyParam = new KeyParameter(hkdfKey);
            keyParamChaChaPoly = new KeyParameter(chachaPolyKey);
        }

        [Benchmark]
        public void ChaCha20Poly1305()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var chacha = new System.Security.Cryptography.ChaCha20Poly1305(chachaPolyKey);
                var tag = new byte[16];

                var encrypted = new byte[data.Length];
                chacha.Encrypt(nonce, data, encrypted, tag);

                var decrypted = new byte[data.Length];
                chacha.Decrypt(nonce, encrypted, tag, decrypted);

                var ismatch = Convert.ToBase64String(decrypted) == Convert.ToBase64String(data);
            });

        }

        [Benchmark]
        public void AesGcm()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var encrypted = AesAdapter.GcmEncryptData(data, hkdfKey, nonce);
                var decrypted = AesAdapter.GcmDecryptData(encrypted.Result, hkdfKey, nonce, encrypted.Tag);

                var ismatch = Convert.ToBase64String(data) == Convert.ToBase64String(decrypted);
            });

        }

        [Benchmark]
        public void AesCcm()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                var encrypted = AesAdapter.CcmEncryptData(data, hkdfKey, nonce);
                var decrypted = AesAdapter.CcmDecryptData(encrypted.Result, hkdfKey, nonce, encrypted.Tag);

                var ismatch = Convert.ToBase64String(data) == Convert.ToBase64String(decrypted);
            });
        }

        [Benchmark]
        public void BouncyChaCha20Poly1305()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                Org.BouncyCastle.Crypto.Modes.ChaCha20Poly1305 cipher = new();
                int macSize = 128;

                // Encrypt
                AeadParameters keyParamAead = new AeadParameters(keyParamChaChaPoly, macSize, nonce, null);
                cipher.Init(true, keyParamAead);
                int outputSize = cipher.GetOutputSize(data.Length);
                byte[] cipherTextData = new byte[outputSize];
                int result = cipher.ProcessBytes(data, 0, data.Length, cipherTextData, 0);
                cipher.DoFinal(cipherTextData, result);

                // Decrypt
                cipher.Init(false, keyParamAead);
                outputSize = cipher.GetOutputSize(cipherTextData.Length);
                var decrypted = new byte[outputSize];
                result = cipher.ProcessBytes(cipherTextData, 0, cipherTextData.Length, decrypted, 0);
                cipher.DoFinal(decrypted, result);


                var ismatch = Convert.ToBase64String(data) == Convert.ToBase64String(decrypted);
            });
        }



        [Benchmark]
        public void BouncyAesGcm()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                IBlockCipher cipher = new AesEngine();
                int macSize = 8 * cipher.GetBlockSize();


                // Encrypt
                AeadParameters keyParamAead = new AeadParameters(keyParam, macSize, nonce, null);
                GcmBlockCipher cipherMode = new GcmBlockCipher(cipher);

                

                cipherMode.Init(true, keyParamAead);
                int outputSize = cipherMode.GetOutputSize(data.Length);
                byte[] cipherTextData = new byte[outputSize];
                int result = cipherMode.ProcessBytes(data, 0, data.Length, cipherTextData, 0);
                cipherMode.DoFinal(cipherTextData, result);
                var rtn = cipherTextData;


                // Decrypt
                cipherMode.Init(false, keyParamAead);
                outputSize = cipherMode.GetOutputSize(cipherTextData.Length);
                var plainTextData = new byte[outputSize];
                result = cipherMode.ProcessBytes(cipherTextData, 0, cipherTextData.Length, plainTextData, 0);
                cipherMode.DoFinal(plainTextData, result);

                var ismatch = Convert.ToBase64String(data) == Convert.ToBase64String(plainTextData);
            });
        }

        [Benchmark]
        public void BouncyAesCcm()
        {
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
            {
                IBlockCipher cipher = new AesEngine();
                int macSize = 8 * cipher.GetBlockSize();

                // Encrypt
                AeadParameters keyParamAead = new AeadParameters(keyParam, macSize, nonce, null);
                CcmBlockCipher cipherMode = new CcmBlockCipher(cipher);
                cipherMode.Init(true, keyParamAead);
                int outputSize = cipherMode.GetOutputSize(data.Length);
                byte[] cipherTextData = new byte[outputSize];
                int result = cipherMode.ProcessBytes(data, 0, data.Length, cipherTextData, 0);
                cipherMode.DoFinal(cipherTextData, result);
                var rtn = cipherTextData;

                // Decrypt
                cipherMode.Init(false, keyParamAead);
                outputSize = cipherMode.GetOutputSize(cipherTextData.Length);
                var plainTextData = new byte[outputSize];
                result = cipherMode.ProcessBytes(cipherTextData, 0, cipherTextData.Length, plainTextData, 0);
                cipherMode.DoFinal(plainTextData, result);

                var ismatch = Convert.ToBase64String(data) == Convert.ToBase64String(plainTextData);
            });
        }
    }
}
