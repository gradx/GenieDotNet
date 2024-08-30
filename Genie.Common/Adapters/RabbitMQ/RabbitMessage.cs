
namespace Genie.Common.Adapters.RabbitMQ
{
    public record RabbitMessage(byte[] Body, string? ReplyTo)
    {

    }
}
