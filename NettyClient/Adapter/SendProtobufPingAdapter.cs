using DotNetty.Transport.Channels;
using Protocol.Protobuf;

namespace NettyClient.Adapter;

public class SendProtobufPingAdapter : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        base.ChannelActive(context);

        context.WriteAndFlushAsync(new Ping
        {
            ChannelId = context.Channel.Id.AsLongText(),
            Data = Random.Shared.Next(),
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        Console.WriteLine($"Protobuf Send: {context.Channel.Id}");
    }
}