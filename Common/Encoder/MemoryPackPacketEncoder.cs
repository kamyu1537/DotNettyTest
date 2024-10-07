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
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    private static readonly DummyBufferWriter Writer = new();
    
    protected override void Encode(IChannelHandlerContext context, IPacket message, IByteBuffer output)
    {
        ArgumentNullException.ThrowIfNull(message);

        var dummy = Writer;
        var array = ArrayPool.Rent(65535);
        var state = MemoryPackWriterOptionalStatePool.Rent(null);

        try
        {
            var writer = new MemoryPackWriter<DummyBufferWriter>(ref dummy, array.AsSpan(), state);
            writer.WriteValue(message);
            var length = writer.WrittenCount;
            writer.Flush();

            output.WriteBytes(array, 0, length);
        }
        finally
        {
            ArrayPool.Return(array);
        }
    }
}