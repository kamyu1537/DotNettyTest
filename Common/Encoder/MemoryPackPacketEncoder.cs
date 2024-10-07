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

        var dummy = Writer;
        using var memoryOwner = MemoryPool.Rent();
        var memory = memoryOwner.Memory;
        
        using var state = MemoryPackWriterOptionalStatePool.Rent(null);

        var writer = new MemoryPackWriter<DummyBufferWriter>(ref dummy, memory.Span, state);
        writer.WriteValue(message);
        var length = writer.WrittenCount;
        writer.Flush();

        output.WriteBytes(memory.ToArray(), 0, length);
    }
}