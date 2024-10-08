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
        byte[]? rentedArray = null;
        
        try
        {
            Span<byte> span;
            if (input.HasArray)
            {
                var array = input.Array;
                if (array is null)
                {
                    throw new InvalidOperationException("array is null");
                }
        
                span = array.AsSpan(input.ArrayOffset + input.ReaderIndex, length);
                input.SetReaderIndex(input.ReaderIndex + length);
                input.MarkReaderIndex();
            }
            else
            {
                rentedArray = ArrayPool.Rent(length);
                input.ReadBytes(rentedArray, 0, length);
                span = rentedArray.AsSpan(0, length);
            }
        
            var packet = MemoryPackSerializer.Deserialize<IPacket>(span);
            if (packet is null)
            {
                throw new InvalidOperationException("packet is null");
            }
        
            output.Add(packet);
        }
        finally
        {
            if (rentedArray is not null)
            {
                ArrayPool.Return(rentedArray);
            }
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine($"MemoryPackPacketDecoder Exception: {exception}");
        context.FireExceptionCaught(exception);
    }
}