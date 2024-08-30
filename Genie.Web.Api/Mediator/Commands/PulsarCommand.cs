using Genie.Common;
using Mediator;
using Microsoft.Extensions.ObjectPool;
using Genie.Web.Api.Common;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Genie.Common.Web;
using Chr.Avro.Abstract;
using Genie.Common.Utils;
using Genie.Common.Types;

namespace Genie.Web.Api.Mediator.Commands;

public record PulsarCommand(ObjectPool<PulsarPooledObject> GeniePool, SchemaBuilder SchemaBuilder, bool FireAndForget) : IRequest;

public class PulsarCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<PulsarCommand>
{

    public EventTaskJob ProcessResult(PulsarCommand command, byte[] message)
    {
        var schema = command.SchemaBuilder.BuildSchema<EventTaskJob>();
        var deserializerBuilder = AvroSupport.GetBinaryDeserializerBuilder();
        var binaryDeserializer = deserializerBuilder.BuildDelegate<EventTaskJob>(schema);

        var reader = new Chr.Avro.Serialization.BinaryReader(message);
        return binaryDeserializer(ref reader);
    }

    public async ValueTask<Unit> Handle(PulsarCommand command, CancellationToken cancellationToken)
    {
        var grpc = MockPartyCreator.GetParty();
        var pooledObj = command.GeniePool.Get();

        if (pooledObj.Counter == 0)
            pooledObj.Configure(Context);

        var bytes = Any.Pack(grpc).ToByteArray();

        if (!command.FireAndForget)
        {
            _ = await pooledObj.Producer!.SendAsync(pooledObj.Producer.NewMessage(bytes, key: command.FireAndForget ? null : pooledObj.EventChannel));
            var message = await pooledObj.Consumer!.ReceiveAsync(cancellationToken);
            //_ = message.GetValue();

            var result = ProcessResult(command, message.GetValue());
            await pooledObj.Consumer.AcknowledgeAsync(message.MessageId);
        }
        else
            await pooledObj.Producer!.SendAndForgetAsync(pooledObj.Producer.NewMessage(bytes, key: command.FireAndForget ? null : pooledObj.EventChannel));

        pooledObj.Counter++;
        command.GeniePool.Return(pooledObj);

        return new Unit();
    }
}