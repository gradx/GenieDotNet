using Genie.Common;
using Mediator;
using Confluent.Kafka;
using Genie.Common.Adapters;
using Confluent.SchemaRegistry;
using Genie.Common.Types;
using Microsoft.Extensions.ObjectPool;
using Genie.Common.Web;
using Utf8StringInterpolation;

namespace Genie.Adapters.Brokers.Kafka;

public record KafkaCommand(ObjectPool<KafkaPooledObject> GeniePool, IAdminClient AdminClient, IProducer<string, PartyRequest> Producer, CachedSchemaRegistryClient SchemaRegistry, bool FireAndForget) : IRequest;

public class KafkaCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<KafkaCommand>
{
    public async ValueTask<Unit> Handle(KafkaCommand command, CancellationToken cancellationToken)
    {

        var grpc = MockPartyCreator.GetParty();
        var partyRequest = CosmosAdapter.ToCosmos(grpc);
        var pooledObj = command.GeniePool.Get();

        if (pooledObj.Counter == 0)
            await pooledObj.Configure(this.Context, command);

        // Kafka still uses the Avro serializer
        var msg = new Message<string, PartyRequest>
        {
            Key = typeof(PartyRequest).Name!,
            Value = partyRequest,
            Headers = []
        };

        if (!command.FireAndForget)
            msg.Headers.Add("EventChannel", Utf8String.Format($"{pooledObj.EventChannel}"));

        await command.Producer.ProduceAsync(this.Context.Kafka.Ingress, msg, cancellationToken);

        EventTaskJob? result = null;
        if (!command.FireAndForget)
        {
            var cr = pooledObj.Consumer!.Consume(cancellationToken);
            result = cr.Message.Value;
        }

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        if (command.FireAndForget)
            return await Task.FromResult(new Unit());
        else if (result?.Status == Genie.Common.Types.EventTaskJobStatus.Errored)
            throw new Exception("Actor Error: " + result?.Exception);
        else
            return new Unit();
    }
}