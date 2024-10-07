using DotNetty.Transport.Channels;
using Protocol.Protobuf;

namespace NettyClient.Adapter;

public class SendProtobufPingAdapter : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        base.ChannelActive(context);

        var message = new Ping
        {
            ChannelId = context.Channel.Id.AsLongText(),
            Data = Random.Shared.Next(),
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        
        context.WriteAndFlushAsync(message);
        Console.WriteLine($"Protobuf {message}");
    }
}