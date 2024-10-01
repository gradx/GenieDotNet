using Genie.Common.Crypto.Adapters.Bouncy;
using Genie.Common.Crypto.Adapters.Curve25519;
using Genie.Common.Crypto.Adapters.Nist;
using Genie.Common.Crypto.Adapters.Pqc;
using Genie.Grpc;
using Genie.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.ObjectPool;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using System.Diagnostics;
using System.IO.Hashing;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Utf8StringInterpolation;

namespace GameLicenseExample;

public class Game(int credits, KeyType signing, KeyType agreement)
{
    public int Credits { get; set; } = credits;
    
    private const string c_URL = "https://luxur.ai:5003";
    private const string c_CERTIFICATE = "luxePod.pfx";

    private readonly int holdRate = 80;
    private readonly Random main = new();
    private readonly Random lower = new();
    private readonly Random upper = new();
    private readonly Random win = new();
    public int ConsecutiveLosses { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }


    public int Risk { get; set; }

    private readonly DefaultObjectPoolProvider objectPoolProvider = new();


    public async Task GetLicense()
    {
        var stringBuilderPool = objectPoolProvider.CreateStringBuilderPool();

        var client = new GeniusEventRPC.GeniusEventRPCClient(GrpcChannel.ForAddress(c_URL, new GrpcChannelOptions
        {
            HttpClient = CreateHttpClient(c_CERTIFICATE),
        }));

        var req = GetMockLicenseRequest(stringBuilderPool, signing, agreement);

        
        await File.WriteAllBytesAsync(@$"{agreement}_{signing}.req", MessageExtensions.ToByteArray(req));

        var sw = new Stopwatch();
        sw.Start();
        var process = client.Process();
        await process.RequestStream.WriteAsync(req);
        await process.RequestStream.CompleteAsync();

        var response = process.ResponseStream;

        while (await response.MoveNext(new CancellationToken()))
        {
            switch(response.Current.ResponseCase)
            {
                case GeniusEventResponse.ResponseOneofCase.License:
                    sw.Stop();
                    Console.WriteLine($@"License request {(response.Current.License.Success ? "accepted" : "denied") } by {response.Current.License.Party.Party.Name} with message: {response.Current.License.Party.Party.CosmosBase.Identifier.Id} in {sw.ElapsedMilliseconds}ms"); ;
                    break;
                case GeniusEventResponse.ResponseOneofCase.Base:
                    sw.Stop();
                    Console.WriteLine($@"License request was {(response.Current.License.Success ? "accepted" : "denied") } with possible errors: { response.Current.Base.Errors.FirstOrDefault()} in {sw.ElapsedMilliseconds}ms");
                    break;
            }
        }

        static GeniusEventRequest GetMockLicenseRequest(ObjectPool<StringBuilder> pool, KeyType signing, KeyType agreement)
        {
            // Create the request
            var req = new GeniusEventRequest
            {
                License = new() { MachineEvent = new() { GeniusEvent = new() {
                            Party = new() {
                                Name = "Skill Game",
                                CosmosBase = new()
                            },
                        }
                    }
                }
            };

            // Provide Geospatial details
            req.License.MachineEvent.GeniusEvent.Party.Communications.Add(new PartyCommunication { CommunicationIdentity = new() { 
                GeographicLocation = new() { GeoJsonLocation = new() { 
                            Circle = new GeoJsonCircle {
                                Centroid = new() { Latitude = 39.9956055995368, Longitude = -75.11449476115337 },
                                Radius = 50
                            }
                        }
                    },
                    Id = "Man Shot and Robbed Playing Skill Game On Kensington Avenue",
                    QualifierValue = "https://delawarevalleynews.com/2024/07/25/man-shot-and-robbed-playing-skill-game-on-kensington-avenue/",
                    Relationship = CommunicationIdentity.Types.CommunicationType.Geospatial
                },
                BeginDate = DateTime.UtcNow.ToTimestamp(),
                LocalityCode = "Philadelphia"
            });

            var cityhash = new XxHash64();
            
            if (agreement == KeyType.Kyber512 || agreement == KeyType.Kyber768 || agreement == KeyType.Kyber1024)
                // Encrypt the message and create the sealed envelope
                req.SealedEnvelope = CreatePqcSealedEnvelope(agreement, pool, "Genie In A Bottle", out byte[] _);
            else
                req.SealedEnvelope = CreateSealedEnvelope(agreement, pool, "Genie In A Bottle",out byte[] _);



            // Hash the request.  Should **NOT** be modified after this
            cityhash.Append(MessageExtensions.ToByteArray(req));

            if (signing == KeyType.Dilithium2 || signing == KeyType.Dilithium3 || signing == KeyType.Dilithium5)
            {
                ProcessDilithium(signing, req, cityhash);
            }
            else if (signing == KeyType.Ed25519 || signing == KeyType.Ed448)
            {
                ProcessCurve25519(signing, req, cityhash);
            }
            else if (signing == KeyType.Secp256K1)
            {
                ProcessKoblitz(signing, req, cityhash);
            }
            else if (signing == KeyType.Secp521R1 || signing == KeyType.Secp384R1 || signing == KeyType.Secp256R1)
            {
                ProcessNist(signing, req, cityhash);
            }

            return req;

            static void ProcessDilithium(KeyType signing, GeniusEventRequest req, XxHash64 hash)
            {
                var channelKey = DilithiumAdapter.Import(new Genie.Common.Types.GeoCryptoKey
                {
                    X509 = File.ReadAllBytes($@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Alice\ModuleLattice\Alice{signing}.key"),
                    IsPrivate = true
                });
                req.SignedParty = new SignedParty { GeoCryptoKeyId = $@"{signing}" };

                // Sign the hash then add the SignedParty to the request
                var sign = Convert.ToBase64String(DilithiumAdapter.Instance.Sign(hash.GetCurrentHash(), channelKey));
                req.SignedParty.Signature = sign;
            }

            static void ProcessCurve25519(KeyType signing, GeniusEventRequest req, XxHash64 hash)
            {
                if (signing == KeyType.Ed25519)
                {
                    var channelKey = Ed25519Adapter.Import(new Genie.Common.Types.GeoCryptoKey
                    {
                        X509 = File.ReadAllBytes($@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Alice\Curve25519\Alice{signing}.key"),
                        IsPrivate = true
                    });

                    req.SignedParty = new SignedParty { GeoCryptoKeyId = $@"{signing}" };

                    // Sign the hash then add the SignedParty to the request
                    var sign = Convert.ToBase64String(Ed25519Adapter.Instance.Sign(hash.GetCurrentHash(), channelKey));
                    req.SignedParty.Signature = sign;
                }
                else
                {
                    var channelKey = Ed448Adapter.Import(new Genie.Common.Types.GeoCryptoKey
                    {
                        X509 = File.ReadAllBytes($@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Alice\Curve25519\Alice{signing}.key"),
                        IsPrivate = true
                    });

                    req.SignedParty = new SignedParty { GeoCryptoKeyId = $@"{signing}" };

                    // Sign the hash then add the SignedParty to the request
                    var sign = Convert.ToBase64String(Ed448Adapter.Instance.Sign(hash.GetCurrentHash(), channelKey));
                    req.SignedParty.Signature = sign;
                }

                
            }

            static void ProcessKoblitz(KeyType signing, GeniusEventRequest req, XxHash64 hash)
            {
                var channelKey = Secp256k1Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
                {
                    X509 = File.ReadAllBytes(@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Alice\Secp\AliceSecp256k1Adapter.key"),
                    IsPrivate = true
                });

                req.SignedParty = new SignedParty { GeoCryptoKeyId = $@"{signing}" };

                // Sign the hash then add the SignedParty to the request
                var sign = Convert.ToBase64String(Secp256k1Adapter.Instance.Sign(hash.GetCurrentHash(), channelKey));
                req.SignedParty.Signature = sign;
            }

            static void ProcessNist(KeyType signing, GeniusEventRequest req, XxHash64 hash)
            {
                var channelKey = Secp521r1Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
                {
                    X509 = File.ReadAllBytes(@$"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Alice\Secp\Bouncy\Alice{signing}.key"),
                    IsPrivate = true
                });

                req.SignedParty = new SignedParty { GeoCryptoKeyId = $@"{signing}" };

                // Sign the hash then add the SignedParty to the request
                var sign = Convert.ToBase64String(Secp256r1Adapter.Instance.Sign(hash.GetCurrentHash(), channelKey));
                req.SignedParty.Signature = sign;
            }
        }
    }

    private static SealedEnvelope CreatePqcSealedEnvelope(KeyType agreementKey, ObjectPool<StringBuilder> pool, string message, out byte[] hkdfKey)
    {
        // Read Bob's public key
        var bob_public_key = (KyberPublicKeyParameters)KyberAdapter.ImportX509(File.ReadAllBytes(@$"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Bob\ModuleLattice\Bob{agreementKey}.cer"));

        // Create a secret with Bob's public Key
        var kyber = KyberAdapter.GenerateSecret(bob_public_key);
        byte[] secret = kyber.GetSecret();

        var key = new GeoCryptoKey
        {
            KeyType = agreementKey,
            IsPrivate = false,
            Id = Guid.NewGuid().ToString("N"),
            PqcE = ByteString.CopyFrom(kyber.GetEncapsulation())
        };

        // Create the AesGcm salt and nonce
        var hkdf_salt = Utf8String.Format($"{StringUtils.RandomString(pool, 16)}");
        var nonce = Utf8String.Format($"{StringUtils.RandomString(pool, 12)}");

        // Create an HDKF key
        var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{key.Id}"));
        hkdfKey = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, hkdf_salt);

        // Encrypt the data with AES GCM
        var (Result, Tag) = AesAdapter.GcmEncryptData(Utf8String.Format($"{message}"), hkdfKey, nonce);

        return new SealedEnvelope
        {
            Key = key,
            Cipher = SealedEnvelope.Types.SealedEnvelopeType.Aes,
            Data = ByteString.CopyFrom(Result),
            Hkdf = ByteString.CopyFrom(hkdf_salt),
            Nonce = ByteString.CopyFrom(nonce),
            Tag = ByteString.CopyFrom(Tag)
        };
    }


    private static SealedEnvelope CreateSealedEnvelope(KeyType agreementKey, ObjectPool<StringBuilder> pool, string message, out byte[] hkdfKey)
    {
        SealedEnvelope envelope = new();
        

        if (agreementKey == KeyType.X25519)
        {
            // Create ephemeral keypair
            var alice_private_keypair = X25519Adapter.GenerateKeyPair();
            var alice_private_key = (X25519PrivateKeyParameters)alice_private_keypair.Private;

            var alice_public_cert = X25519Adapter.ExportX509PublicCertificate(alice_private_keypair, "Genie PKI").RawData;

            var key = new GeoCryptoKey
            {
                X509 = ByteString.CopyFrom(alice_public_cert),
                KeyType = agreementKey,
                IsPrivate = false,
                Id = Guid.NewGuid().ToString("N")
            };

            // Read Bob's public key
            var bob_certificate = new X509Certificate2(File.ReadAllBytes(@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Bob\Curve25519\X25519Adapter.cer"));
            var bob_public_key = new X25519PublicKeyParameters(bob_certificate.GetPublicKey(), 0);

            // Create a secret with Alice's private key and Bob's public Key
            byte[] secret = new byte[32];
            alice_private_key.GenerateSecret(bob_public_key, secret, 0);

            ProcessSecret(pool, message, out hkdfKey, out envelope, key, secret);
        }
        else if (agreementKey == KeyType.X448)
        {
            // Create ephemeral keypair
            var alice_private_keypair = X448Adapter.GenerateKeyPair();
            var alice_private_key = (X448PrivateKeyParameters)alice_private_keypair.Private;

            var alice_public_cert = X448Adapter.ExportX509PublicCertificate(alice_private_keypair, "Genie PKI").RawData;

            var key = new GeoCryptoKey
            {
                X509 = ByteString.CopyFrom(alice_public_cert),
                KeyType = agreementKey,
                IsPrivate = false,
                Id = Guid.NewGuid().ToString("N")
            };

            // Read Bob's public key
            var bob_certificate = new X509Certificate2(File.ReadAllBytes(@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Bob\Curve25519\BobX448.cer"));
            var bob_public_key = new X448PublicKeyParameters(bob_certificate.GetPublicKey(), 0);

            // Create a secret with Alice's private key and Bob's public Key
            byte[] secret = new byte[64];
            alice_private_key.GenerateSecret(bob_public_key, secret, 0);

            ProcessSecret(pool, message, out hkdfKey, out envelope, key, secret);
        }
        else if (agreementKey == KeyType.Secp256K1)
        {
            // Create ephemeral keypair
            var alice_private_keypair = Secp256k1Adapter.Instance.GenerateKeyPair();
            var alice_private_key = (ECPrivateKeyParameters)alice_private_keypair.Private;

            var alice_public_cert = Secp256k1Adapter.ExportX509PublicCertificate(alice_private_keypair, "Genie PKI").RawData;

            var key = new GeoCryptoKey
            {
                X509 = ByteString.CopyFrom(alice_public_cert),
                KeyType = agreementKey,
                IsPrivate = false,
                Id = Guid.NewGuid().ToString("N")
            };

            // Read Bob's public key
            var bob_certificate = Secp256k1Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
            {
                X509 = File.ReadAllBytes(@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Bob\Secp\BobSecp256k1Adapter.cer"),
                IsPrivate = false
            });

            // Create a secret with Alice's private key and Bob's public Key
            byte[] secret = Secp256k1Adapter.CreateSecret(alice_private_key, (ECPublicKeyParameters)bob_certificate);
            ProcessSecret(pool, message, out hkdfKey, out envelope, key, secret);
        }
        else // Secp*r1  
        {
            // Create ephemeral keypair
            var alice_private_keypair = (agreementKey == KeyType.Secp256R1) ? Secp256r1Adapter.Instance.GenerateKeyPair() :
                (agreementKey == KeyType.Secp384R1) ? Secp384r1Adapter.Instance.GenerateKeyPair() :
                Secp521r1Adapter.Instance.GenerateKeyPair();
            var alice_private_key = (ECPrivateKeyParameters)alice_private_keypair.Private;

            var alice_public_cert = Secp256r1Adapter.Instance.Export(alice_private_keypair.Public, false);

            var key = new GeoCryptoKey
            {
                X509 = ByteString.CopyFrom(alice_public_cert),
                KeyType = agreementKey,
                IsPrivate = false,
                Id = Guid.NewGuid().ToString("N")
            };

            // Read Bob's public key
            var bob_certificate = Secp256r1Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
            {
                X509 = File.ReadAllBytes(@$"C:\Users\gradx\repos\GenieDotNet\SharedFiles\Keys\Bob\Secp\Bouncy\Bob{agreementKey}.cer"),
                IsPrivate = false
            });

            // Create a secret with Alice's private key and Bob's public Key
            byte[] secret = Secp256r1Adapter.CreateSecret(alice_private_key, (ECPublicKeyParameters)bob_certificate);

            ProcessSecret(pool, message, out hkdfKey, out envelope, key, secret);
        }

        return envelope;

        static string RandomString(ObjectPool<StringBuilder> pool, int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@";
            var res = pool.Get();

            //res.Length = length + 1;

            while (length-- > 0)
            {
                var rng = RandomNumberGenerator.GetBytes(sizeof(uint));
                uint num = BitConverter.ToUInt32(rng, 0);
                res.Append(valid[(int)(num % (uint)valid.Length)]);
            }

            var result =  res.ToString();
            pool.Return(res);
            return result;
        }

        static void ProcessSecret(ObjectPool<StringBuilder> pool, string message, out byte[] hkdfKey, out SealedEnvelope envelope, GeoCryptoKey key, byte[] secret)
        {
            // Create the AesGcm salt and nonce
            var hkdf_salt = Utf8String.Format($"{RandomString(pool, 16)}");
            var nonce = Utf8String.Format($"{RandomString(pool, 12)}");

            // Create an HDKF key
            var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{key.Id}"));
            hkdfKey = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, hkdf_salt);

            // Encrypt the data with AES GCM
            var (Result, Tag) = AesAdapter.GcmEncryptData(Utf8String.Format($"{message}"), hkdfKey, nonce);

            envelope = new SealedEnvelope
            {
                Key = key,
                Cipher = SealedEnvelope.Types.SealedEnvelopeType.Aes,
                Data = ByteString.CopyFrom(Result),
                Hkdf = ByteString.CopyFrom(hkdf_salt),
                Nonce = ByteString.CopyFrom(nonce),
                Tag = ByteString.CopyFrom(Tag)
            };
        }
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

    public int Play()
    {
        if (this.Risk > Credits)
            return -1; // negative indicates play

        Credits -= this.Risk;


        var nextLower = lower.Next(0 - holdRate, 0);
        var nextUpper = upper.Next(0, 100 - (holdRate / 2));


        var result = main.Next(
            nextLower,
            nextUpper
        );

        var winAmount = Math.Max(win.Next(0 - this.Risk, this.Risk * 5), this.Risk);

        this.GamesPlayed++;
        if (result > 0)
        {
            this.ConsecutiveLosses = 0;
            GamesWon++;
            // calculate win amount
           
            this.Credits += winAmount;
        }
        else
            this.ConsecutiveLosses++;

        if (result > 0)
            return winAmount;
        else
            return 0;
    }

    public int Exit()
    {
        var ret = this.Credits;
        this.Credits = 0;

        return ret;
    }

    public void Deposit(int amount)
    {
        this.Credits += amount;
    }
}