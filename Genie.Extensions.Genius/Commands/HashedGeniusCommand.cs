using Genie.Actors;
using Genie.Common;
using Genie.Common.Adapters;
using Genie.Common.Crypto.Adapters.Bouncy;
using Genie.Common.Crypto.Adapters.Curve25519;
using Genie.Common.Crypto.Adapters.Nist;
using Genie.Common.Crypto.Adapters.Pqc;
using Genie.Common.Performance;
using Genie.Common.Web;
using Genie.Grpc;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using NetTopologySuite.Utilities;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Proto;
using Proto.Cluster;
using System.Buffers;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Utf8StringInterpolation;

namespace Genie.Extensions.Genius.Commands;

public record HashedGeniusCommand(IAsyncStreamReader<GeniusEventRequest> Request, IServerStreamWriter<GeniusEventResponse> Response, ServerCallContext Context, ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget) : IRequest;

public class HashedGeniusCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<HashedGeniusCommand>
{
    private static readonly RecyclableMemoryStreamManager manager = new();


    public async ValueTask<Unit> Handle(HashedGeniusCommand command, CancellationToken cancellationToken)
    {
        // Inheriting from ActorCommand causes it to route to only ActorCommandHandler
        // MissingMessageHandlerException: No handler registered for message type: Genie.Web.Api.Mediator.Commands.GeniusCommand

        //var grpc = MockPartyCreator.GetParty();
        //grpc.Party.CosmosBase.Identifier.Id = "Help";

        // command.HttpContext.Connection.RemoteIpAddress?.ToString()
        //grpc.Request.IpAddressSource
        //grpc.Request.IpAddressDestination

        var pooledObj = command.GeniePool.Get();


        do
        {
            if (command.Request.Current == null)
                continue;


            var response = await ProcessRequest(command, pooledObj, command.Request.Current, cancellationToken);

            await command.Response.WriteAsync(response!.Response.Unpack<GeniusEventResponse>(), cancellationToken);


        } while (await command.Request.MoveNext());

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        return new Unit();

        static async Task<string> ProcessSecret(HashedGeniusCommand command, byte[] secret, CancellationToken cancellationToken)
        {
            var envelope = command.Request.Current.SealedEnvelope;
            var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{envelope.Key.Id}"));
            var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, [.. envelope.Hkdf]);

            // Decrypt the Data
            var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key, [.. envelope.Nonce], [.. envelope.Tag]).ToArray();
            RecyclableMemoryStream ms = manager.GetStream();
            await ms.WriteAsync(decrypted_bytes, cancellationToken);
            ms.Position = 0;
            StreamReader reader = new StreamReader(ms);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }

    static async ValueTask<GeniusResponse?> ProcessRequest(HashedGeniusCommand command, GeniePooledObject pooledObj, GeniusEventRequest grpc, CancellationToken cancellationToken)
    {
        var signedparty = grpc.SignedParty?.Signature;

        if (string.IsNullOrEmpty(signedparty))
        {
            var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
            resp.Base.Errors.Add("Signature failed");
            throw new Exception("Failed");
        }

        var cityhash = new XxHash64();
        var signedPartyOrig = grpc.SignedParty.Adapt<SignedParty>();

        grpc.SignedParty = null;
        cityhash.Append(MessageExtensions.ToByteArray(grpc));


        if (signedPartyOrig.GeoCryptoKeyId.ToString().StartsWith("Dilithium"))
        {
            var signature_verified = DilithiumAdapter.Verify2(cityhash.GetCurrentHash(),
                Convert.FromBase64String(signedparty),
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = false,
                    X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Keys\Alice\Alice{signedPartyOrig.GeoCryptoKeyId}.cer")
                }));

            if (!signature_verified)
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                return null;
            }
        }
        else if (signedPartyOrig.GeoCryptoKeyId == KeyType.Ed25519.ToString())
        {
            var signature_verified = Ed25519Adapter.Instance.Verify(cityhash.GetCurrentHash(),
            Convert.FromBase64String(signedparty),
            Ed25519Adapter.Import(new Common.Types.GeoCryptoKey
            {
                IsPrivate = false,
                X509 = File.ReadAllBytes("AliceEd25519")
            }));

            if (!signature_verified)
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                return null;
            }
        }
        else if (signedPartyOrig.GeoCryptoKeyId == KeyType.Ed448.ToString())
        {
            var signature_verified = Ed448Adapter.Instance.Verify(cityhash.GetCurrentHash(),
            Convert.FromBase64String(signedparty),
            Ed448Adapter.Import(new Common.Types.GeoCryptoKey
            {
                IsPrivate = false,
                X509 = File.ReadAllBytes("AliceEd448")
            }));

            if (!signature_verified)
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                return null;
            }
        }
        else if (signedPartyOrig.GeoCryptoKeyId == KeyType.Secp256K1.ToString())
        {
            var signature_verified = Secp256k1Adapter.Instance.Verify(cityhash.GetCurrentHash(),
                Convert.FromBase64String(signedparty),
                Secp256k1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = false,
                    X509 = File.ReadAllBytes("AliceSecp256k1")
                }));

            if (!signature_verified)
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                return null;
            }
        }
        else
        {
            var signature_verified = Secp256r1Adapter.Instance.Verify(cityhash.GetCurrentHash(),
                    Convert.FromBase64String(signedparty),
                    Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = false,
                        X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Keys\Alice\Alice{signedPartyOrig.GeoCryptoKeyId}.cer")
                    }));

            if (!signature_verified)
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                return null;
            }
        }

        if (grpc.SealedEnvelope != null)
        {
            if (grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber512 || grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber768 || grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber1024)
            {
                var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;

                // Import Bob's Private Key
                var bob_private_key = (KyberPrivateKeyParameters)KyberAdapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = true,
                    X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Keys\Bob\Bob{grpc.SealedEnvelope.Key.KeyType}.key")
                });

                Mutex kyber_m = new Mutex(false, "kyber");
                kyber_m.WaitOne();

                // Create an extractor with Bob's private key
                var kyber = new KyberKemExtractor(bob_private_key);

                // Recreate the secret generated with the Encapsulation
                var secret = kyber.ExtractSecret(sealed_envelope.PqcE);

                kyber_m.ReleaseMutex();

                // Extract the HKDF key
                var envelope = grpc.SealedEnvelope;

                string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                // Proof
                Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
            }
            else if (grpc.SealedEnvelope.Key.KeyType == KeyType.X25519)
            {
                // Import Alice's Key
                var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                var alice_public_key = X25519Adapter.GetX25519PublicKeyParameters(new X509Certificate2(sealed_envelope.X509!));

                // Import Bob's Private Key
                var bob_private_key = (X25519PrivateKeyParameters)X25519Adapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = true,
                    X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Keys\Bob\Bob{grpc.SealedEnvelope.Key.KeyType}.key")
                });

                // Recreate the secret generated from Alice's private key and Bob's public Key
                var secret = new byte[32];
                bob_private_key.GenerateSecret(alice_public_key, secret, 0);

                // Extract the HKDF key
                var envelope = grpc.SealedEnvelope;

                string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                // Proof
                Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
            }
            else if (grpc.SealedEnvelope.Key.KeyType == KeyType.X448)
            {
                // Import Alice's Key
                var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                var alice_public_key = X448Adapter.GetX448ublicKeyParameters(new X509Certificate2(sealed_envelope.X509!));

                // Import Bob's Private Key
                var bob_private_key = (X448PrivateKeyParameters)X448Adapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = true,
                    X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Keys\Bob\Bob{grpc.SealedEnvelope.Key.KeyType}.key")
                });

                // Recreate the secret generated from Alice's private key and Bob's public Key
                var secret = new byte[32];
                bob_private_key.GenerateSecret(alice_public_key, secret, 0);

                // Extract the HKDF key
                var envelope = grpc.SealedEnvelope;

                string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                // Proof
                Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
            }
            else if (grpc.SealedEnvelope.Key.KeyType == KeyType.Secp256K1)
            {
                // Import Alice's Key
                var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                var alice_public_key = Secp256k1Adapter.Instance.ImportX509(sealed_envelope.X509!);

                // Import Bob's Private Key
                var bob_private_key = (ECPrivateKeyParameters)Secp256k1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = true,
                    X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Keys\Bob\Bob{grpc.SealedEnvelope.Key.KeyType}.key")
                });

                // Recreate the secret generated from Alice's private key and Bob's public Key
                var secret = Secp256k1Adapter.CreateSecret((ECPrivateKeyParameters)bob_private_key, (ECPublicKeyParameters)alice_public_key);

                // Extract the HKDF key
                var envelope = grpc.SealedEnvelope;

                string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                // Proof
                Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
            }
            else
            {
                // Import Alice's Key
                var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                //var alice_public_key = Secp256r1Adapter.Instance.ImportX509(sealed_envelope.X509!);
                var alice_public_key = Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = sealed_envelope.X509! });

                // Import Bob's Private Key
                var bob_private_key = (ECPrivateKeyParameters)Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = true,
                    X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Keys\Bob\Bob{grpc.SealedEnvelope.Key.KeyType}.key")
                });

                // Recreate the secret generated from Alice's private key and Bob's public Key
                var secret = Secp256r1Adapter.CreateSecret((ECPrivateKeyParameters)bob_private_key, (ECPublicKeyParameters)alice_public_key);

                // Extract the HKDF key
                var envelope = grpc.SealedEnvelope;

                string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                // Proof
                Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
            }
        }

        var actor = await ActorCommandHandler.HandleCommand(new Grpc.PartyRequest
        {
            Party = grpc.License.MachineEvent.GeniusEvent.Party,
            Request = new BaseRequest
            {
                CosmosBase = new CosmosBase { },
                Origin = new Coordinate { Latitude = 39.995638476704855, Longitude = -75.11468788019755, Altitude = 0 },
            }
        },
            new ActorCommand(command.GeniePool, command.ActorSystem, command.FireAndForget, null), cancellationToken);

        return await InitiateActor(command.ActorSystem, new GeniusRequest
        {
            Value = actor?.Response,
            Timestamp = DateTime.UtcNow.ToTimestamp(),
            Request = new Genius.StatusRequest
            {
                Topic = pooledObj.EventChannel,
                Offset = pooledObj.Counter
            },


        }, command.FireAndForget, cancellationToken);

        static async Task<GeniusResponse?> InitiateActor(ActorSystem actorSystem, GeniusRequest request, bool fireAndForget, CancellationToken cancellationToken)
        {
            var grainClient = actorSystem.Cluster().GetGeniusService("help");
            if (fireAndForget)
            {
                _ = grainClient.Process(request, cancellationToken);
                return null;
            }
            else
                return await grainClient.Process(request, new CancellationToken());
        }

        static async Task<string> ProcessEnvelope(byte[] secret, SealedEnvelope envelope, CancellationToken cancellationToken)
        {
            var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{envelope.Key.Id}"));
            var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, [.. envelope.Hkdf]);

            // Decrypt the Data
            var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key, [.. envelope.Nonce], [.. envelope.Tag]).ToArray();
            RecyclableMemoryStream ms = manager.GetStream();
            await ms.WriteAsync(decrypted_bytes, cancellationToken);
            ms.Position = 0;
            StreamReader reader = new StreamReader(ms);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}


public record NetworkBenchmarkHashedGeniusCommand(MessageParser<GeniusEventRequest> Parser, HttpContext HttpContext, ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget) : IRequest;

public class NetworkBenchmarkGeniusCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<NetworkBenchmarkHashedGeniusCommand>
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    public async ValueTask<Unit> Handle(NetworkBenchmarkHashedGeniusCommand command, CancellationToken cancellationToken)
    {
        using var str = manager.GetStream();
        await command.HttpContext.Request.BodyReader.CopyToAsync(str, cancellationToken);

        var hashed = new BenchmarkGeniusCommandHandler(this.Context);
        return await hashed.Handle(new BenchmarkHashedGeniusCommand(command.Parser, str.GetReadOnlySequence().ToArray(), command.HttpContext, command.GeniePool, command.ActorSystem, command.FireAndForget), cancellationToken);
    }
}

public record BenchmarkHashedGeniusCommand(MessageParser<GeniusEventRequest> Parser, byte[] File, HttpContext HttpContext, ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget) : IRequest;

public class BenchmarkGeniusCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<BenchmarkHashedGeniusCommand>
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    private static readonly byte[] AliceDilithium2 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceDilithium2.cer");
    private static readonly byte[] AliceDilithium3 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceDilithium3.cer");
    private static readonly byte[] AliceDilithium5 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceDilithium5.cer");

    private static readonly byte[] AliceEd25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Ed25519SigningAdapter.cer");
    private static readonly byte[] AliceEd448 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceEd448.cer");

    private static readonly byte[] AliceSecp256k1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceSecp256k1Adapter.cer");
    private static readonly byte[] AliceSecp256R1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceSecp256R1.cer");
    private static readonly byte[] AliceSecp384R1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceSecp384R1.cer");
    private static readonly byte[] AliceSecp521R1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\AliceSecp521R1.cer");

    private static readonly byte[] BobX25519 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\X25519Adapter.key");
    private static readonly byte[] BobX448 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobX448.key");

    private static readonly byte[] BobKyber512 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobKyber512.key");
    private static readonly byte[] BobKyber768 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobKyber768.key");
    private static readonly byte[] BobKyber1024 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobKyber1024.key");

    private static readonly byte[] BobSecp256k1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobSecp256k1Adapter.key");
    private static readonly byte[] BobSecp256R1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobSecp256R1.key");
    private static readonly byte[] BobSecp384R1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobSecp384R1.key");
    private static readonly byte[] BobSecp521R1 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\BobSecp521R1.key");


    public async ValueTask<Unit> Handle(BenchmarkHashedGeniusCommand command, CancellationToken cancellationToken)
    {
        // Inheriting from ActorCommand causes it to route to only ActorCommandHandler
        // MissingMessageHandlerException: No handler registered for message type: Genie.Web.Api.Mediator.Commands.GeniusCommand

        //var grpc = MockPartyCreator.GetParty();
        //grpc.Party.CosmosBase.Identifier.Id = "Help";

        // command.HttpContext.Connection.RemoteIpAddress?.ToString()
        //grpc.Request.IpAddressSource
        //grpc.Request.IpAddressDestination

        GeniePooledObject pooledObj = command.GeniePool.Get();


        var grpc = command.Parser.ParseFrom(command.File);

        var result = await ProcessRequest(command, pooledObj, grpc, cancellationToken);

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        return new Unit();



        static async ValueTask<GeniusResponse?> ProcessRequest(BenchmarkHashedGeniusCommand command, GeniePooledObject pooledObj, GeniusEventRequest grpc, CancellationToken cancellationToken)
        {
            var signedparty = grpc.SignedParty?.Signature;

            if (string.IsNullOrEmpty(signedparty))
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                throw new Exception("Failed");
            }

            var cityhash = new XxHash64();

            var signedPartyOrig = grpc.SignedParty.Adapt<SignedParty>();

            grpc.SignedParty = null;
            cityhash.Append(MessageExtensions.ToByteArray(grpc));


            if (signedPartyOrig.GeoCryptoKeyId == KeyType.Dilithium2.ToString() || signedPartyOrig.GeoCryptoKeyId == KeyType.Dilithium3.ToString() || signedPartyOrig.GeoCryptoKeyId == KeyType.Dilithium5.ToString())
            {
                var signature_verified = DilithiumAdapter.Verify2(cityhash.GetCurrentHash(),
                    Convert.FromBase64String(signedparty),
                    DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = false,
                        X509 = (signedPartyOrig.GeoCryptoKeyId == KeyType.Dilithium2.ToString()) ? AliceDilithium2 : (signedPartyOrig.GeoCryptoKeyId == KeyType.Dilithium3.ToString()) ? AliceDilithium3 : AliceDilithium5
                    }));

                if (!signature_verified)
                {
                    var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                    resp.Base.Errors.Add("Signature failed");
                    return null;
                }
            }
            else if (signedPartyOrig.GeoCryptoKeyId == KeyType.Ed25519.ToString())
            {
                var signature_verified = Ed25519Adapter.Instance.Verify(cityhash.GetCurrentHash(),
                Convert.FromBase64String(signedparty),
                Ed25519Adapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = false,
                    X509 = AliceEd25519
                }));

                if (!signature_verified)
                {
                    var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                    resp.Base.Errors.Add("Signature failed");
                    return null;
                }
            }
            else if (signedPartyOrig.GeoCryptoKeyId == KeyType.Ed448.ToString())
            {
                var signature_verified = Ed448Adapter.Instance.Verify(cityhash.GetCurrentHash(),
                Convert.FromBase64String(signedparty),
                Ed448Adapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = false,
                    X509 = AliceEd448
                }));

                if (!signature_verified)
                {
                    var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                    resp.Base.Errors.Add("Signature failed");
                    return null;
                }
            }
            else if (signedPartyOrig.GeoCryptoKeyId == KeyType.Secp256K1.ToString())
            {
                var signature_verified = Secp256k1Adapter.Instance.Verify(cityhash.GetCurrentHash(),
                    Convert.FromBase64String(signedparty),
                    Secp256k1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = false,
                        X509 = AliceSecp256k1
                    }));

                if (!signature_verified)
                {
                    var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                    resp.Base.Errors.Add("Signature failed");
                    return null;
                }
            }
            else
            {
                var signature_verified = Secp256r1Adapter.Instance.Verify(cityhash.GetCurrentHash(),
                        Convert.FromBase64String(signedparty),
                        Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                        {
                            IsPrivate = false,
                            X509 = (signedPartyOrig.GeoCryptoKeyId == KeyType.Secp256R1.ToString()) ? AliceSecp256R1 : (signedPartyOrig.GeoCryptoKeyId == KeyType.Secp384R1.ToString()) ? AliceSecp384R1 : AliceSecp521R1
                        }));

                if (!signature_verified)
                {
                    var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                    resp.Base.Errors.Add("Signature failed");
                    return null;
                }
            }

            if (grpc.SealedEnvelope != null)
            {
                if (grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber512 || grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber768 || grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber1024)
                {
                    var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;

                    // Import Bob's Private Key
                    var bob_private_key = (KyberPrivateKeyParameters)KyberAdapter.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = true,
                        X509 = (grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber512) ? BobKyber512 : (grpc.SealedEnvelope.Key.KeyType == KeyType.Kyber768) ? BobKyber768 : BobKyber1024
                    });

                    Mutex kyber_m = new Mutex(false, "kyber");
                    kyber_m.WaitOne();

                    // Create an extractor with Bob's private key
                    var kyber = new KyberKemExtractor(bob_private_key);

                    // Recreate the secret generated with the Encapsulation
                    var secret = kyber.ExtractSecret(sealed_envelope.PqcE);

                    kyber_m.ReleaseMutex();

                    // Extract the HKDF key
                    var envelope = grpc.SealedEnvelope;

                    string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                    // Proof
                    Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
                }
                else if (grpc.SealedEnvelope.Key.KeyType == KeyType.X25519)
                {
                    // Import Alice's Key
                    var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                    var alice_public_key = X25519Adapter.GetX25519PublicKeyParameters(new X509Certificate2(sealed_envelope.X509!));

                    // Import Bob's Private Key
                    var bob_private_key = (X25519PrivateKeyParameters)X25519Adapter.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = true,
                        X509 = BobX25519
                    });

                    // Recreate the secret generated from Alice's private key and Bob's public Key
                    var secret = new byte[32];
                    bob_private_key.GenerateSecret(alice_public_key, secret, 0);

                    // Extract the HKDF key
                    var envelope = grpc.SealedEnvelope;

                    string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                    // Proof
                    Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
                }
                else if (grpc.SealedEnvelope.Key.KeyType == KeyType.X448)
                {
                    // Import Alice's Key
                    var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                    var alice_public_key = X448Adapter.GetX448ublicKeyParameters(new X509Certificate2(sealed_envelope.X509!));

                    // Import Bob's Private Key
                    var bob_private_key = (X448PrivateKeyParameters)X448Adapter.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = true,
                        X509 = BobX448
                    });

                    // Recreate the secret generated from Alice's private key and Bob's public Key
                    var secret = new byte[64];
                    bob_private_key.GenerateSecret(alice_public_key, secret, 0);

                    // Extract the HKDF key
                    var envelope = grpc.SealedEnvelope;

                    string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                    // Proof
                    Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
                }
                else if (grpc.SealedEnvelope.Key.KeyType == KeyType.Secp256K1)
                {
                    // Import Alice's Key
                    var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                    var alice_public_key = Secp256k1Adapter.Instance.ImportX509(sealed_envelope.X509!);

                    // Import Bob's Private Key
                    var bob_private_key = (ECPrivateKeyParameters)Secp256k1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = true,
                        X509 = BobSecp256k1
                    });

                    // Recreate the secret generated from Alice's private key and Bob's public Key
                    var secret = Secp256k1Adapter.CreateSecret((ECPrivateKeyParameters)bob_private_key, (ECPublicKeyParameters)alice_public_key);

                    // Extract the HKDF key
                    var envelope = grpc.SealedEnvelope;

                    string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                    // Proof
                    Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
                }
                else
                {
                    // Import Alice's Key
                    var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                    //var alice_public_key = Secp256r1Adapter.Instance.ImportX509(sealed_envelope.X509!);
                    var alice_public_key = Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false, X509 = sealed_envelope.X509! });

                    // Import Bob's Private Key
                    var bob_private_key = (ECPrivateKeyParameters)Secp256r1Adapter.Instance.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = true,
                        X509 = (grpc.SealedEnvelope.Key.KeyType == KeyType.Secp256R1) ? BobSecp256R1 : (grpc.SealedEnvelope.Key.KeyType == KeyType.Secp384R1) ? BobSecp384R1 : BobSecp521R1
                    });

                    // Recreate the secret generated from Alice's private key and Bob's public Key
                    var secret = Secp256r1Adapter.CreateSecret((ECPrivateKeyParameters)bob_private_key, (ECPublicKeyParameters)alice_public_key);

                    // Extract the HKDF key
                    var envelope = grpc.SealedEnvelope;

                    string decrypted_string_message = await ProcessEnvelope(secret, envelope, cancellationToken);

                    // Proof
                    Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
                }
            }

            var actor = await ActorCommandHandler.HandleCommand(new Grpc.PartyRequest
            {
                Party = grpc.License.MachineEvent.GeniusEvent.Party,
                Request = new BaseRequest
                {
                    CosmosBase = new CosmosBase { },
                    Origin = new Coordinate { Latitude = 39.995638476704855, Longitude = -75.11468788019755, Altitude = 0 },
                }
            },
                new ActorCommand(command.GeniePool, command.ActorSystem, command.FireAndForget, null), cancellationToken);

            return await InitiateActor(command.ActorSystem, new GeniusRequest
            {
                Value = actor?.Response,
                Timestamp = DateTime.UtcNow.ToTimestamp(),
                Request = new Genius.StatusRequest
                {
                    Topic = pooledObj.EventChannel,
                    Offset = pooledObj.Counter
                },


            }, command.FireAndForget, cancellationToken);

            static async Task<GeniusResponse?> InitiateActor(ActorSystem actorSystem, GeniusRequest request, bool fireAndForget, CancellationToken cancellationToken)
            {
                var grainClient = actorSystem.Cluster().GetGeniusService("help");
                if (fireAndForget)
                {
                    _ = grainClient.Process(request, cancellationToken);
                    return null;
                }
                else
                    return await grainClient.Process(request, new CancellationToken());
            }

            static async Task<string> ProcessEnvelope(byte[] secret, SealedEnvelope envelope, CancellationToken cancellationToken)
            {
                var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{envelope.Key.Id}"));
                var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, [.. envelope.Hkdf]);

                // Decrypt the Data
                var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key, [.. envelope.Nonce], [.. envelope.Tag]).ToArray();
                RecyclableMemoryStream ms = manager.GetStream();
                await ms.WriteAsync(decrypted_bytes, cancellationToken);
                ms.Position = 0;
                StreamReader reader = new StreamReader(ms);
                return await reader.ReadToEndAsync(cancellationToken);
            }
        }
    }
}