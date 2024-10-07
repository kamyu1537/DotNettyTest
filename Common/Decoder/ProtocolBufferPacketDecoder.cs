using System.Buffers;
using System.Collections.Concurrent;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Protocol.Protobuf;

namespace Common.Decoder;

public class ProtocolBufferPacketDecoder : ByteToMessageDecoder
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

            var packet = Packet.Parser.ParseFrom(array.AsSpan(0, length));
            var field = Packet.Descriptor.FindFieldByNumber((int)packet.DataCase);
            if (field?.Accessor.GetValue(packet) is not IMessage message)
            {
                throw new InvalidOperationException("message not found");
            }

            output.Add(message);
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