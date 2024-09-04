using Genie.Actors;
using Genie.Common;
using Genie.Common.Adapters;
using Genie.Common.Crypto.Adapters.Curve25519;
using Genie.Common.Crypto.Adapters.Nist;
using Genie.Common.Crypto.Adapters.Pqc;
using Genie.Common.Performance;
using Genie.Common.Web;
using Genie.Grpc;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using NetTopologySuite.Utilities;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using Proto;
using Proto.Cluster;
using System.Data.HashFunction.CityHash;
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


            var signedparty = command.Request.Current.SignedParty?.Signature;

            if (string.IsNullOrEmpty(signedparty))
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                await command.Response.WriteAsync(resp, cancellationToken);
                break;
            }

            var cityhash = CityHashFactory.Instance.Create(new CityHashConfig { HashSizeInBits = 64 });

            command.Request.Current.SignedParty = null;
            var hash = cityhash.ComputeHash(MessageExtensions.ToByteArray(command.Request.Current), cancellationToken);

            if(command.Request.Current.SealedEnvelope.Key.KeyType == KeyType.Dilithium)
            {
                var signature_verified = DilithiumAdapter.Instance.Verify(hash.Hash,
                    Convert.FromBase64String(signedparty),
                    DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = false,
                        KeyType = Common.Types.GeoCryptoKey.CryptoKeyType.Dilithium,
                        //Key = Convert.ToBase64String(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Ed25519SigningAdapter.cer")),
                        X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\DilithiumAdapter.cer")
                    }));

                if (!signature_verified)
                {
                    var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                    resp.Base.Errors.Add("Signature failed");
                    await command.Response.WriteAsync(resp, cancellationToken);
                    break;
                }
            }
            else
            {
                var signature_verified = Ed25519Adapter.Instance.Verify(hash.Hash,
                Convert.FromBase64String(signedparty),
                Ed25519Adapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = false,
                    KeyType = Common.Types.GeoCryptoKey.CryptoKeyType.Ed25519,
                    //Key = Convert.ToBase64String(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Ed25519SigningAdapter.cer")),
                    X509 = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Ed25519SigningAdapter.cer")
                }));

                if (!signature_verified)
                {
                    var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                    resp.Base.Errors.Add("Signature failed");
                    await command.Response.WriteAsync(resp, cancellationToken);
                    break;
                }
            }



            if (command.Request.Current.SealedEnvelope != null)
            {
                if (command.Request.Current.SealedEnvelope.Key.PqcE.Length != 0)
                {
                    // Import Alice's Key
                    var sealed_envelope = CosmosAdapter.ToCosmos(command.Request.Current.SealedEnvelope)!;
                    var alice_public_key = (KyberPublicKeyParameters)KyberAdapter.ImportX509(sealed_envelope.X509!);

                    // Import Bob's Private Key
                    var bob_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\KyberAdapter.key");
                    var bob_private_key = (KyberPrivateKeyParameters)KyberAdapter.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = true,
                        X509 = bob_key
                    });

                    // Recreate the secret generated from Bob's private key and Alice's public Key
                    var kyber = new KyberKemExtractor(bob_private_key);
                    var secret = kyber.ExtractSecret(sealed_envelope.PqcE);


                    // Extract the HKDF key
                    var envelope = command.Request.Current.SealedEnvelope;
                    var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{envelope.Key.Id}"));
                    var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, [.. envelope.Hkdf]);

                    // Decrypt the Data
                    var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key, [.. envelope.Nonce], [.. envelope.Tag]).ToArray();
                    using var ms = manager.GetStream();
                    await ms.WriteAsync(decrypted_bytes, cancellationToken);
                    ms.Position = 0;
                    using var reader = new StreamReader(ms);
                    var decrypted_string_message = await reader.ReadToEndAsync(cancellationToken);

                    // Proof
                    Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
                }
                else
                {
                    // Import Alice's Key
                    var sealed_envelope = CosmosAdapter.ToCosmos(command.Request.Current.SealedEnvelope)!;
                    var alice_public_key = X25519Adapter.GetX25519PublicKeyParameters(new X509Certificate2(sealed_envelope.X509!));

                    // Import Bob's Private Key
                    var bob_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\X25519Adapter.key");
                    var bob_private_key = (X25519PrivateKeyParameters)X25519Adapter.Import(new Common.Types.GeoCryptoKey
                    {
                        IsPrivate = true,
                        X509 = bob_key
                    });

                    // Recreate the secret generated from Alice's private key and Bob's public Key
                    var secret = new byte[32];
                    bob_private_key.GenerateSecret(alice_public_key, secret, 0);

                    // Extract the HKDF key
                    var envelope = command.Request.Current.SealedEnvelope;
                    var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{envelope.Key.Id}"));
                    var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, [.. envelope.Hkdf]);

                    // Decrypt the Data
                    var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key, [.. envelope.Nonce], [.. envelope.Tag]).ToArray();
                    using var ms = manager.GetStream();
                    await ms.WriteAsync(decrypted_bytes, cancellationToken);
                    ms.Position = 0;
                    using var reader = new StreamReader(ms);
                    var decrypted_string_message = await reader.ReadToEndAsync(cancellationToken);

                    // Proof
                    Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
                }

            }


            var actor = await ActorCommandHandler.HandleCommand(new Grpc.PartyRequest
            {
                Party = command.Request.Current.License.MachineEvent.GeniusEvent.Party,
                Request = new BaseRequest
                {
                     CosmosBase = new CosmosBase { },
                     Origin = new Coordinate { Latitude = 39.995638476704855, Longitude = -75.11468788019755, Altitude = 0 },
                }
            }, 
                new ActorCommand(command.GeniePool, command.ActorSystem, command.FireAndForget, null), cancellationToken);

            var response = await InitiateActor(command.ActorSystem, new GeniusRequest
            {
                Value = actor?.Response,
                Timestamp = DateTime.UtcNow.ToTimestamp(),
                Request = new Genius.StatusRequest
                {
                    Topic = pooledObj.EventChannel,
                    Offset = pooledObj.Counter
                },


            }, command.FireAndForget, cancellationToken);

            await command.Response.WriteAsync(response!.Response.Unpack<GeniusEventResponse>(), cancellationToken);


        } while (await command.Request.MoveNext());

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        return new Unit();
    }

    private static async Task<GeniusResponse?> InitiateActor(ActorSystem actorSystem, GeniusRequest request, bool fireAndForget, CancellationToken cancellationToken)
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
}


public record NetworkBenchmarkHashedGeniusCommand(MessageParser<GeniusEventRequest> Parser, HttpContext HttpContext, ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget) : IRequest;

public class NetworkBenchmarkGeniusCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<NetworkBenchmarkHashedGeniusCommand>
{
    public async ValueTask<Unit> Handle(NetworkBenchmarkHashedGeniusCommand command, CancellationToken cancellationToken)
    {
        var hashed = new BenchmarkGeniusCommandHandler(this.Context);
        var request = await hashed.ProcessArtifacts(command.Parser, command.HttpContext.Request, cancellationToken);

        return await hashed.Handle(new BenchmarkHashedGeniusCommand(command.Parser, request.Grpc.ToByteArray(), command.HttpContext, command.GeniePool, command.ActorSystem, command.FireAndForget), cancellationToken);
    }
}

public record BenchmarkHashedGeniusCommand(MessageParser<GeniusEventRequest> Parser, byte[] File, HttpContext HttpContext, ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget) : IRequest;

public class BenchmarkGeniusCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<BenchmarkHashedGeniusCommand>
{
    private static readonly RecyclableMemoryStreamManager manager = new();

    private static readonly byte[] AliceDilithiumAdaptercer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\DilithiumAdapter.cer");
    private static readonly byte[] AliceEd25519SigningAdaptercer = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Ed25519SigningAdapter.cer");
    private static readonly byte[] BobKyberAdapterkey = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\KyberAdapter.key");
    private static readonly byte[] BobX25519Adapterkey = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\X25519Adapter.key");
    public async ValueTask<Unit> Handle(BenchmarkHashedGeniusCommand command, CancellationToken cancellationToken)
    {
        // Inheriting from ActorCommand causes it to route to only ActorCommandHandler
        // MissingMessageHandlerException: No handler registered for message type: Genie.Web.Api.Mediator.Commands.GeniusCommand

        //var grpc = MockPartyCreator.GetParty();
        //grpc.Party.CosmosBase.Identifier.Id = "Help";

        // command.HttpContext.Connection.RemoteIpAddress?.ToString()
        //grpc.Request.IpAddressSource
        //grpc.Request.IpAddressDestination

        var pooledObj = command.GeniePool.Get();            

        var grpc = command.Parser.ParseFrom(command.File);

        var signedparty = grpc.SignedParty?.Signature;

        if (string.IsNullOrEmpty(signedparty))
        {
            var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
            resp.Base.Errors.Add("Signature failed");
            throw new Exception("Failed");
        }

        var cityhash = CityHashFactory.Instance.Create(new CityHashConfig { HashSizeInBits = 64 });

        grpc.SignedParty = null;
        var hash = cityhash.ComputeHash(MessageExtensions.ToByteArray(grpc), cancellationToken);

        if (grpc.SealedEnvelope.Key.KeyType == KeyType.Dilithium)
        {
            var signature_verified = DilithiumAdapter.Instance.Verify(hash.Hash,
                Convert.FromBase64String(signedparty),
                DilithiumAdapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = false,
                    KeyType = Common.Types.GeoCryptoKey.CryptoKeyType.Dilithium,
                    X509 = AliceDilithiumAdaptercer
                }));

            if (!signature_verified)
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                return new Unit();
            }
        }
        else
        {
            var signature_verified = Ed25519Adapter.Instance.Verify(hash.Hash,
            Convert.FromBase64String(signedparty),
            Ed25519Adapter.Import(new Common.Types.GeoCryptoKey
            {
                IsPrivate = false,
                KeyType = Common.Types.GeoCryptoKey.CryptoKeyType.Ed25519,
                X509 = AliceEd25519SigningAdaptercer
            }));

            if (!signature_verified)
            {
                var resp = new GeniusEventResponse { Base = new BaseResponse { Success = false } };
                resp.Base.Errors.Add("Signature failed");
                return new Unit();
            }
        }

        if (grpc.SealedEnvelope != null)
        {
            if (grpc.SealedEnvelope.Key.PqcE.Length != 0)
            {
                // Import Alice's Key
                var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                var alice_public_key = (KyberPublicKeyParameters)KyberAdapter.ImportX509(sealed_envelope.X509!);

                // Import Bob's Private Key
                var bob_private_key = (KyberPrivateKeyParameters)KyberAdapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = true,
                    X509 = BobKyberAdapterkey
                });


                Mutex broken = new Mutex(false, "kyber");
                broken.WaitOne();
                // Recreate the secret generated from Bob's private key and Alice's public Key
                var kyber = new KyberKemExtractor(bob_private_key);
                var secret = kyber.ExtractSecret(sealed_envelope.PqcE);
                broken.ReleaseMutex();

                // Extract the HKDF key
                var envelope = grpc.SealedEnvelope;
                var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{envelope.Key.Id}"));
                var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, [.. envelope.Hkdf]);

                // Decrypt the Data
                var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key, [.. envelope.Nonce], [.. envelope.Tag]).ToArray();
                using var ms = manager.GetStream();
                await ms.WriteAsync(decrypted_bytes, cancellationToken);
                ms.Position = 0;
                using var reader = new StreamReader(ms);
                var decrypted_string_message = await reader.ReadToEndAsync(cancellationToken);

                // Proof
                Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
            }
            else
            {
                // Import Alice's Key
                var sealed_envelope = CosmosAdapter.ToCosmos(grpc.SealedEnvelope)!;
                var alice_public_key = X25519Adapter.GetX25519PublicKeyParameters(new X509Certificate2(sealed_envelope.X509!));

                // Import Bob's Private Key
                var bob_private_key = (X25519PrivateKeyParameters)X25519Adapter.Import(new Common.Types.GeoCryptoKey
                {
                    IsPrivate = true,
                    X509 = BobX25519Adapterkey
                });

                // Recreate the secret generated from Alice's private key and Bob's public Key
                var secret = new byte[32];
                bob_private_key.GenerateSecret(alice_public_key, secret, 0);

                // Extract the HKDF key
                var envelope = grpc.SealedEnvelope;
                var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Utf8String.Format($"{envelope.Key.Id}"));
                var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, [.. envelope.Hkdf]);

                // Decrypt the Data
                var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key, [.. envelope.Nonce], [.. envelope.Tag]).ToArray();
                using var ms = manager.GetStream();
                await ms.WriteAsync(decrypted_bytes, cancellationToken);
                ms.Position = 0;
                using var reader = new StreamReader(ms);
                var decrypted_string_message = await reader.ReadToEndAsync(cancellationToken);

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

        var response = await InitiateActor(command.ActorSystem, new GeniusRequest
        {
            Value = actor?.Response,
            Timestamp = DateTime.UtcNow.ToTimestamp(),
            Request = new Genius.StatusRequest
            {
                Topic = pooledObj.EventChannel,
                Offset = pooledObj.Counter
            },


        }, command.FireAndForget, cancellationToken);

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        return new Unit();
    }

    private static async Task<GeniusResponse?> InitiateActor(ActorSystem actorSystem, GeniusRequest request, bool fireAndForget, CancellationToken cancellationToken)
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
}