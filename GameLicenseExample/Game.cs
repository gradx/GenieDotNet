
using Genie.Common.Crypto.Adapters;
using Genie.Grpc;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Org.BouncyCastle.Crypto.Parameters;
using System.Data.HashFunction.CityHash;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
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


    public async Task GetLicense()
    {
        var client = new GeniusEventRPC.GeniusEventRPCClient(GrpcChannel.ForAddress(c_URL, new GrpcChannelOptions
        {
            HttpClient = CreateHttpClient(c_CERTIFICATE),
        }));

        var req = GetMockLicenseRequest();

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

        static GeniusEventRequest GetMockLicenseRequest()
        {
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


            req.SealedEnvelope = CreateEnvelope("Genie In A Bottle", @"Keys\Bob\X25519Adapter.cer", out byte[] _);


            // Hash request
            var cityhash = CityHashFactory.Instance.Create(new CityHashConfig { HashSizeInBits = 64 });
            var hash = cityhash.ComputeHash(MessageExtensions.ToByteArray(req));

            var channelKey = Ed25519Adapter.Instance.Import(new Genie.Common.Types.GeoCryptoKey
            {
                Key = Convert.ToBase64String(File.ReadAllBytes(@"Keys\Alice\Ed25519SigningAdapter.key")),
                KeyType = Genie.Common.Types.GeoCryptoKey.CryptoKeyType.Ed25519,
                IsPrivate = true
            });

            // Sign the hash then add the SignedParty to the request
            var sign = Convert.ToBase64String(Ed25519Adapter.Instance.Sign(hash.Hash, channelKey));
            req.SignedParty = new SignedParty { Signature = sign };

            return req;
        }
    }

    private static SealedEnvelope CreateEnvelope(string message, string keyPath, out byte[] hdkfKey)
    {
        var ephemeral_key = X25519Adapter.Instance.GenerateKeyPair();
        var eph_x25_priv = (X25519PrivateKeyParameters)ephemeral_key.Private;
        _ = (X25519PublicKeyParameters)ephemeral_key.Public;

        var key = new GeoCryptoKey
        {
            Key = Convert.ToBase64String(X25519Adapter.Instance.Export(ephemeral_key.Private, true)),
            KeyType = KeyType.X25519,
            IsPrivate = true,
            Id = "New Thread"
        };

        // Read Bob's (channel) public key and generate a secret
        var bob = new X509Certificate2(File.ReadAllBytes(keyPath));
        var bob_x25_pub = new X25519PublicKeyParameters(bob.GetPublicKey(), 0);

        byte[] secret = new byte[32];
        eph_x25_priv.GenerateSecret(bob_x25_pub, secret, 0);

        // Encrypt the data
        var geniune = Encoding.UTF8.GetBytes(message);
        string hkdf_salt = RandomString(16);
        string nonce = RandomString(12);

        var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Encoding.UTF8.GetBytes("partyId"));
        hdkfKey = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, Encoding.UTF8.GetBytes(hkdf_salt));
        var envelope = AesAdapter.EncryptData(geniune, Convert.ToBase64String(hdkfKey), nonce);

        return new SealedEnvelope
        {
            Key = key,
            Cipher = SealedEnvelope.Types.SealedEnvelopeType.Aes,
            Data = Convert.ToBase64String(envelope.Result),
            Hkdf = hkdf_salt,
            Nonce = nonce,
            Tag = Convert.ToBase64String(envelope.Tag)
        };

        static string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@";
            StringBuilder res = new(length);

            while (length-- > 0)
            {
                var rng = RandomNumberGenerator.GetBytes(sizeof(uint));
                uint num = BitConverter.ToUInt32(rng, 0);
                res.Append(valid[(int)(num % (uint)valid.Length)]);
            }

            return res.ToString();
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