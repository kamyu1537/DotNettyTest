using System.Buffers;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Protocol.Protobuf;

namespace Common.Decoder;

public class ProtocolBufferPacketDecoder : ByteToMessageDecoder
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

            var packet = Packet.Parser.ParseFrom(span);
            var field = Packet.Descriptor.FindFieldByNumber((int)packet.DataCase);
            if (field?.Accessor.GetValue(packet) is not IMessage message)
            {
                throw new InvalidOperationException("message not found");
            }

            output.Add(message);
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
        Console.WriteLine($"ProtocolBufferPacketDecoder Exception: {exception}");
        context.FireExceptionCaught(exception);
    }
}