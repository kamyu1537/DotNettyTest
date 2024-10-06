using DotNetty.Transport.Channels;
using Protocol.MessagePack;
using Protocol.MessagePack.Packet;

namespace NettyClient.InboundHandler;

public class MessagePackMessageHandler : SimpleChannelInboundHandler<IPacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, IPacket msg)
    {
        _ = HandleMessageAsync(ctx, msg).ConfigureAwait(false);
    }
    
    private static async Task HandleMessageAsync(IChannelHandlerContext context, IPacket message)
    {
        if (message is PongPacket packet)
        {
            Console.WriteLine($"MessagePack {packet.ChannelId}: {packet.Data} {packet.Time}");    
        }
        
        await Task.Delay(10);
        await context.WriteAndFlushAsync(new PingPacket
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