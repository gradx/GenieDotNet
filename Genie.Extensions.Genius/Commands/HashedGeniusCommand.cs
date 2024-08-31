using Mediator;
using Proto;
using Genie.Common;
using Genie.Actors;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.ObjectPool;
using Genie.Common.Web;
using Proto.Cluster;
using Genie.Grpc;
using Grpc.Core;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Google.Protobuf;
using System.Data.HashFunction.CityHash;
using Genie.Common.Crypto.Adapters;
using Org.BouncyCastle.Crypto.Parameters;
using Genie.Common.Adapters;
using System.Security.Cryptography;
using NetTopologySuite.Utilities;
using Genie.Common.Performance;
using Utf8StringInterpolation;
using Microsoft.IO;
using System.Security.Cryptography.X509Certificates;

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

            var signature_verified = Ed25519Adapter.Instance.Verify(hash.Hash,
                Convert.FromBase64String(signedparty),
                Ed25519Adapter.Import(new Common.Types.GeoCryptoKey { IsPrivate = false,
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

            if (command.Request.Current.SealedEnvelope != null)
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
                var decrypted_bytes = AesAdapter.GcmDecryptData([.. envelope.Data], hkdf_key,[.. envelope.Nonce], [.. envelope.Tag]).ToArray();
                using var ms = manager.GetStream();
                await ms.WriteAsync(decrypted_bytes, cancellationToken);
                ms.Position = 0;
                using var reader = new StreamReader(ms);               
                var decrypted_string_message = await reader.ReadToEndAsync(cancellationToken);

                // Proof
                Assert.IsTrue(decrypted_string_message == "Genie In A Bottle");
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
