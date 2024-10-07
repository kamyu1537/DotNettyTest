using System.Buffers;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MemoryPack;
using Protocol.MemoryPack;

namespace Common.Decoder;

public class MemoryPackPacketDecoder : ByteToMessageDecoder
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        ArgumentNullException.ThrowIfNull(input);

        var length = input.ReadableBytes;
        var array = ArrayPool.Rent(length);

        try
        {
            input.ReadBytes(array, 0, length);
            var packet = MemoryPackSerializer.Deserialize<IPacket>(array.AsSpan(0, length));
            if (packet is null)
            {
                throw new InvalidOperationException("packet is null");
            }

            output.Add(packet);
        }
        finally
        {
            ArrayPool.Return(array);
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        context.FireExceptionCaught(exception);
    }
}