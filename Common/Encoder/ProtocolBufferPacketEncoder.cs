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
    private static readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;
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
        using var memoryOwner = MemoryPool.Rent(length);

        var memory = memoryOwner.Memory;
        packet.WriteTo(memory.Span);

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
}