using DotNetty.Transport.Channels;

namespace Common.Adapter;

public class ConnectionLogAdapter : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine($"Client connected: {context.Channel.RemoteAddress}");
        base.ChannelActive(context);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine($"Client disconnected: {context.Channel.RemoteAddress}");
        base.ChannelInactive(context);
    }
}