using System.Buffers;
using Common.Buffer;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MemoryPack;
using Protocol.MemoryPack;

namespace Common.Encoder;

public class MemoryPackPacketEncoder : MessageToByteEncoder<IPacket>
{
    private static readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;
    private static readonly DummyBufferWriter Writer = new();

    protected override void Encode(IChannelHandlerContext context, IPacket message, IByteBuffer output)
    {
        ArgumentNullException.ThrowIfNull(message);

        var dummyBufferWriter = Writer;
        
        using var memoryPackWriterState = MemoryPackWriterOptionalStatePool.Rent(null);
        using var memoryOwner = MemoryPool.Rent();
        var memory = memoryOwner.Memory;
        var writer = new MemoryPackWriter<DummyBufferWriter>(ref dummyBufferWriter, memory.Span, memoryPackWriterState);
        try
        {
            writer.WriteValue(message);
            var length = writer.WrittenCount;
            var span = memory.Span[..length];
            if (output.HasArray)
            {
                output.EnsureWritable(length);
                var startIndex = output.ArrayOffset + output.WriterIndex;
                span.CopyTo(output.Array.AsSpan(startIndex, length));
                output.SetWriterIndex(output.WriterIndex + length);
            }
            else
            {
                output.WriteBytes(span.ToArray());
            }
        }
        finally
        {
            writer.Flush();
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        context.FireExceptionCaught(exception);
    }
}