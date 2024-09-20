
namespace Genie.Adapters.Brokers.RabbitMQ;

public record RabbitMessage(byte[] Body, string? ReplyTo)
{

}