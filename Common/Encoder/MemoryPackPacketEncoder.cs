using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MemoryPack;
using Protocol.MemoryPack;

namespace Common.Encoder;

public class MemoryPackPacketEncoder : MessageToByteEncoder<IPacket>
{
    protected override void Encode(IChannelHandlerContext context, IPacket message, IByteBuffer output)
    {
        ArgumentNullException.ThrowIfNull(message);
        var bytes = MemoryPackSerializer.Serialize(message);
        output.WriteBytes(bytes);
    }
}