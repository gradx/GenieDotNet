
using Cysharp.IO;
using Genie.Common.Crypto.Adapters;
using Genie.Grpc;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.ObjectPool;
using Org.BouncyCastle.Crypto.Parameters;
using System.Data.HashFunction.CityHash;
using System.IO;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using Utf8StringInterpolation;
using static Azure.Core.HttpHeader;

namespace GameLicenseExample;

 public class Game(int credits)
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

        var req = GetMockLicenseRequest(stringBuilderPool);

        var process = client.Process();
        await process.RequestStream.WriteAsync(req);
        await process.RequestStream.CompleteAsync();

        var response = process.ResponseStream;

        while (await response.MoveNext(new CancellationToken()))
        {
            switch(response.Current.ResponseCase)
            {
                case GeniusEventResponse.ResponseOneofCase.License:
                    Console.WriteLine($@"License request {(response.Current.License.Success ? "accepted" : "denied") } by {response.Current.License.Party.Party.Name} with message: {response.Current.License.Party.Party.CosmosBase.Identifier.Id}"); ;
                    break;
                case GeniusEventResponse.ResponseOneofCase.Base:
                    Console.WriteLine($@"License request was {(response.Current.License.Success ? "accepted" : "denied") } with possible errors: { response.Current.Base.Errors.FirstOrDefault()}");
                    break;
            }
        }

        static GeniusEventRequest GetMockLicenseRequest(ObjectPool<StringBuilder> pool)
        {
            // Create the request
            var req = new GeniusEventRequest
            {
                License = new()
                {
                    MachineEvent = new()
                    {
                        GeniusEvent = new()
                        {
                            Party = new()
                            {
                                Name = "Skill Game",
                                CosmosBase = new()
                            },
                        }
                    }
                }
            };

            // Provide Geospatial details
            req.License.MachineEvent.GeniusEvent.Party.Communications.Add(new PartyCommunication
            {
                CommunicationIdentity = new()
                {
                    GeographicLocation = new()
                    {
                        GeoJsonLocation = new()
                        {
                            Circle = new GeoJsonCircle
                            {
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

            // Encrypt the message and create the sealed envelope
            req.SealedEnvelope = CreateSealedEnvelope(pool, "Genie In A Bottle", @"Keys\Bob\X25519Adapter.cer", out byte[] _);

            // Hash the request
            var cityhash = CityHashFactory.Instance.Create(new CityHashConfig { HashSizeInBits = 64 });
            var hash = cityhash.ComputeHash(MessageExtensions.ToByteArray(req));

            var channelKey = Ed25519Adapter.Import(new Genie.Common.Types.GeoCryptoKey
            {
                //Key = Convert.ToBase64String(File.ReadAllBytes(@"Keys\Alice\Ed25519SigningAdapter.key")),
                X509 = File.ReadAllBytes(@"Keys\Alice\Ed25519SigningAdapter.key"),
                KeyType = Genie.Common.Types.GeoCryptoKey.CryptoKeyType.Ed25519,
                IsPrivate = true
            });

            // Sign the hash then add the SignedParty to the request
            var sign = Convert.ToBase64String(Ed25519Adapter.Instance.Sign(hash.Hash, channelKey));
            req.SignedParty = new SignedParty { Signature = sign };

            return req;
        }
    }

    private static SealedEnvelope CreateSealedEnvelope(ObjectPool<StringBuilder> pool, string message, string keyPath, out byte[] hkdfKey)
    {

        var alice_private_keypair = X25519Adapter.GenerateKeyPair();
        var alice_private_key = (X25519PrivateKeyParameters)alice_private_keypair.Private;
        _ = (X25519PublicKeyParameters)alice_private_keypair.Public;

        var alice_public_cert = X25519Adapter.ExportX509PublicCertificate(alice_private_keypair, "Genie PKI").RawData;

        var key = new GeoCryptoKey
        {
            //Key = Convert.ToBase64String(alice_public_cert),
            X509 = ByteString.CopyFrom(alice_public_cert),
            KeyType = KeyType.X25519,
            IsPrivate = false,
            Id = Guid.NewGuid().ToString("N")
        };

        // Read Bob's public key
        var bob_certificate = new X509Certificate2(File.ReadAllBytes(keyPath));
        var bob_public_key = new X25519PublicKeyParameters(bob_certificate.GetPublicKey(), 0);

        // Create a secret with Alice's private key and Bob's public Key
        byte[] secret = new byte[32];
        alice_private_key.GenerateSecret(bob_public_key, secret, 0);

        // Create the AesGcm salt and nonce
        var hkdf_salt = Utf8String.Format($"{RandomString(pool, 16)}");
        var nonce = Utf8String.Format($"{RandomString(pool, 12)}");

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