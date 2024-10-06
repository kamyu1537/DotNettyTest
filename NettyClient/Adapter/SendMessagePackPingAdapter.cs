using DotNetty.Transport.Channels;
using Protocol.MessagePack.Packet;

namespace NettyClient.Adapter;

public class SendMessagePackPingAdapter : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        base.ChannelActive(context);

        context.WriteAndFlushAsync(new PingPacket
        {
            ChannelId = context.Channel.Id.AsLongText(),
            Data = Random.Shared.Next(),
            Time = DateTime.UtcNow
        });
        
        Console.WriteLine($"MessagePack Send: {context.Channel.Id}");
    }
}