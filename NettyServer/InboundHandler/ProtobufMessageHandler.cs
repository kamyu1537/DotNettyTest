using DotNetty.Transport.Channels;
using Google.Protobuf;
using Protocol.Protobuf;

namespace NettyServer.InboundHandler;

public class ProtobufMessageHandler : SimpleChannelInboundHandler<IMessage>
{
    protected override void ChannelRead0(IChannelHandlerContext context, IMessage message)
    {
        _ = HandleMessageAsync(context, message).ConfigureAwait(false);
    }

    private static async Task HandleMessageAsync(IChannelHandlerContext context, IMessage message)
    {
#if DEBUG
        Console.WriteLine($"Protobuf {message}");
#endif
        
        await Task.Delay(10);
        await context.WriteAndFlushAsync(new Pong
        {
            ChannelId = context.Channel.Id.AsLongText(),
            Data = Random.Shared.Next(),
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine("[MessageHandler] ExceptionCaught: " + exception);
        context.FireExceptionCaught(exception);
    }
}