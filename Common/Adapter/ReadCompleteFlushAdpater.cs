using DotNetty.Transport.Channels;

namespace Common.Adapter;

public class ReadCompleteFlushAdapter : ChannelHandlerAdapter
{
    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        context.Flush();
        base.ChannelReadComplete(context);
    }
}