using System.Buffers;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using MemoryPack;
using Protocol.MemoryPack;

namespace Common.Codec;

public class MemoryPackPacketCodec : MessageToMessageCodec<IByteBuffer, IPacket>
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Create();

    protected override void Encode(IChannelHandlerContext context, IPacket input, List<object> output)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        var writer = new ArrayBufferWriter<byte>(65536);
        MemoryPackSerializer.Serialize(writer, input);

        var length = writer.WrittenCount;
        var buffer = context.Allocator.Buffer(length);
        var array = ArrayPool.Rent(length);

        try
        {
            writer.WrittenSpan.CopyTo(array.AsSpan());
            buffer.WriteBytes(array, 0, length);
            output.Add(buffer);
        }
        catch
        {
            buffer.SafeRelease();
            throw;
        }
        finally
        {
            ArrayPool.Return(array);
        }
    }

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
        base.ExceptionCaught(context, exception);
        Console.WriteLine(exception);
    }
}