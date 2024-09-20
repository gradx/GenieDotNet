using Mediator;
using Genie.Common;
using Genie.Grpc;
using Genie.Common.Web;
using Microsoft.Extensions.Logging;


namespace Genie.Adapters.Brokers.ZeroMQ;

public record ZloggerCommand(ILogger<PartyRequest> Logger) : IRequest;

public class ZloggerCommandHandler(GenieContext genieContext) : BaseCommandHandler(genieContext), IRequestHandler<ZloggerCommand>
{
    public async ValueTask<Unit> Handle(ZloggerCommand command, CancellationToken cancellationToken)
    {
        var grpc = MockPartyCreator.GetParty();


        command.Logger.LogInformation($"{grpc}");
        await Task.CompletedTask;
        return new Unit();
    }
}
