using DotNetty.Transport.Channels;

namespace Common.Adapter;

public class ExceptionLogAdapter : ChannelHandlerAdapter
{
    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
        base.ExceptionCaught(ctx, ex);
    }
}