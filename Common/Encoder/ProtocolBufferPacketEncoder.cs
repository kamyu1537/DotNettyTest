using System.Buffers;
using System.Collections.Concurrent;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Protocol.Protobuf;

namespace Common.Encoder;

public class ProtocolBufferPacketEncoder : MessageToByteEncoder<IMessage>
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Create();
    private static readonly ConcurrentDictionary<MessageDescriptor, FieldDescriptor> FieldDescriptorCache = new(Packet.Descriptor.Fields.InDeclarationOrder().ToDictionary(x => x.MessageType, x => x));
    
    protected override void Encode(IChannelHandlerContext context, IMessage message, IByteBuffer output)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!FieldDescriptorCache.TryGetValue(message.Descriptor, out var field))
        {
            throw new InvalidOperationException("field not found");
        }

        var packet = new Packet();
        field.Accessor.SetValue(packet, message);

        var length = packet.CalculateSize();
        var array = ArrayPool.Rent(length);
        
        try
        {
            packet.WriteTo(array.AsSpan(0, length));
            output.WriteBytes(array, 0, length);
        }
        finally
        {
            ArrayPool.Return(array);
        }
    }
}