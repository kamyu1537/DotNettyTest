using System.Buffers;
using System.Collections.Concurrent;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Protocol.Protobuf;

namespace Common.Codec;

public class ProtobufPacketCodec : MessageToMessageCodec<IByteBuffer, IMessage>
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Create();
    private static readonly ConcurrentDictionary<MessageDescriptor, FieldDescriptor> FieldDescriptorCache = new(Packet.Descriptor.Fields.InDeclarationOrder().ToDictionary(x => x.MessageType, x => x));

    protected override void Encode(IChannelHandlerContext context, IMessage input, List<object> output)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!FieldDescriptorCache.TryGetValue(input.Descriptor, out var field))
        {
            throw new InvalidOperationException("field not found");
        }

        var packet = new Packet();
        field.Accessor.SetValue(packet, input);

        var length = packet.CalculateSize();
        var buffer = context.Allocator.Buffer(length);
        var array = ArrayPool.Rent(length);
        
        try
        {
            packet.WriteTo(array.AsSpan(0, length));
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
        base.ExceptionCaught(context, exception);
        Console.WriteLine(exception);
    }
}