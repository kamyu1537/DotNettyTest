using DotNetty.Transport.Channels;
using Protocol.MemoryPack;
using Protocol.MemoryPack.Packet;

namespace NettyServer.InboundHandler;

public class MemoryPackMessageHandler : SimpleChannelInboundHandler<IPacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, IPacket msg)
    {
        _ = HandleMessageAsync(ctx, msg).ConfigureAwait(false);
    }

    private static async Task HandleMessageAsync(IChannelHandlerContext context, IPacket message)
    {
#if DEBUG
        if (message is PingPacket packet)
        {
            Console.WriteLine($"MemoryPack {packet.ChannelId}: {packet.Data} {packet.Time}");    
        }
#endif
        
        await Task.Delay(10);
        await context.WriteAndFlushAsync(new PongPacket
        {
            ChannelId = context.Channel.Id.AsLongText(),
            Data = Random.Shared.Next(),
            Time = DateTime.UtcNow
        });
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine(exception);
        context.CloseAsync();
    }
}