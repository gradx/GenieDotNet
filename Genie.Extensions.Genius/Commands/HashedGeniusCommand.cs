using Mediator;
using Proto;
using Genie.Common;
using Genie.Actors;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.ObjectPool;
using Microsoft.AspNetCore.Http;
using Genie.Common.Web;
using Genie.Extensions.Genius;
using Proto.Cluster;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Genie.Common.Utils;
using Genie.Grpc;
using Microsoft.Azure.Cosmos.Spatial;
using Grpc.Core;
using Chr.Avro.Confluent;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using CommunityToolkit.HighPerformance;
using Google.Protobuf;
using System.Data.HashFunction.CityHash;
using Genie.Common.Crypto.Adapters;
using System.Text;
using System;
using Org.BouncyCastle.Crypto.Parameters;
using Genie.Common.Adapters;
using System.Security.Cryptography;
using NetTopologySuite.Utilities;
using Genie.Common.Performance;

namespace Genie.Extensions.Commands;

public record HashedGeniusCommand(IAsyncStreamReader<GeniusEventRequest> Request, IServerStreamWriter<GeniusEventResponse> Response, ServerCallContext Context, ObjectPool<GeniePooledObject> GeniePool, ActorSystem ActorSystem, bool FireAndForget) : IRequest;

public class HashedGeniusCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<HashedGeniusCommand>
{
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
                Ed25519Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = false,
                    KeyType = Common.Types.GeoCryptoKey.CryptoKeyType.Ed25519,
                    Key = Convert.ToBase64String(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Alice\Ed25519SigningAdapter.cer"))
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
                var bob_key = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Keys\Bob\X25519Adapter.key");
                var privateKey = (X25519PrivateKeyParameters)X25519Adapter.Instance.Import(new Common.Types.GeoCryptoKey { IsPrivate = true, 
                    Key = Convert.ToBase64String(bob_key) });


                var adapter = (X25519PrivateKeyParameters)X25519Adapter.Instance.Import(CosmosAdapter.ToCosmos(command.Request.Current.SealedEnvelope)!);
                var secret = new byte[32];
                privateKey.GenerateSecret((X25519PublicKeyParameters)adapter.GeneratePublicKey(), secret, 0);

                var extract = HKDF.Extract(HashAlgorithmName.SHA256, secret, Encoding.UTF8.GetBytes("partyId"));
                var hkdf_key = HKDF.Expand(HashAlgorithmName.SHA256, extract, 24, Encoding.UTF8.GetBytes(command.Request.Current.SealedEnvelope.Hkdf));
                var decrypted = AesAdapter.DecryptData(Convert.FromBase64String(command.Request.Current.SealedEnvelope.Data), Convert.ToBase64String(hkdf_key),
                    command.Request.Current.SealedEnvelope.Nonce, Convert.FromBase64String(command.Request.Current.SealedEnvelope.Tag));

                var decrypted_message = Encoding.UTF8.GetString(decrypted);

                Assert.IsTrue(decrypted_message == "Genie In A Bottle");
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
            return await grainClient.Process(request, cancellationToken);
    }
}
