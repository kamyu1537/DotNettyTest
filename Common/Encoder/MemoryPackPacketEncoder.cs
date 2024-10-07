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
        using var memoryOwner = MemoryPool.Rent();
        using var memoryPackWriterState = MemoryPackWriterOptionalStatePool.Rent(null);

        var memory = memoryOwner.Memory;
        var writer = new MemoryPackWriter<DummyBufferWriter>(ref dummyBufferWriter, memory.Span, memoryPackWriterState);
        
        try
        {
            writer.WriteValue(message);
            var length = writer.WrittenCount;
            
            if (output.HasArray)
            {
                output.EnsureWritable(length);
                var startIndex = output.ArrayOffset + output.WriterIndex;
                memory.Span[..length].CopyTo(output.Array.AsSpan(startIndex, length));
                output.SetWriterIndex(output.WriterIndex + length);
            }
            else
            {
#if DEBUG
                Console.WriteLine("MemoryPackPacketEncoder: output does not have array");
#endif
                output.WriteBytes(memory.Span[..length].ToArray());
            }
        }
        finally
        {
            writer.Flush();
        }
    }
}