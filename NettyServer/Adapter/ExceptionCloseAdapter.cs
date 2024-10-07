using DotNetty.Transport.Channels;

namespace NettyServer.Adapter;

public class ExceptionCloseAdapter : ChannelHandlerAdapter
{
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        context.CloseAsync();
        context.FireExceptionCaught(exception);
    }
}