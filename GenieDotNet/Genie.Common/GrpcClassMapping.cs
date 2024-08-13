using Google.Protobuf;

namespace Genie.Common;


public class GrpcClassMapping
{
    public static Type? GetType(IMessage message)
    {
        return null;
        //return message switch
        //{
        //    GRPC.PartyRequest => typeof(Types.ChannelController.ChannelMemberRequest),
        //    GRPC.MessageRequest => typeof(Types.MessageController.MessageRequest),
        //    GRPC.ChannelRequest => typeof(Types.ChannelController.ChannelRequest),
        //    GRPC.DirectMessageRequest => typeof(Types.MessageController.DirectMessageRequest),
        //    _ => throw new NullReferenceException("Message type not supported")
        //};
    }

    public static Type? GetType(dynamic message)
    {
        return null;
        //return message.GetType() switch
        //{
        //    Types.ChannelController.ChannelMemberRequest => typeof(ChannelMemberRequest),
        //    Types.MessageController.MessageRequest => typeof(MessageRequest),
        //    Types.ChannelController.ChannelRequest => typeof(ChannelRequest),
        //    Types.MessageController.DirectMessageRequest => typeof(DirectMessageRequest),
        //    _ => throw new NullReferenceException("Message type not supported")
        //};
    }
}