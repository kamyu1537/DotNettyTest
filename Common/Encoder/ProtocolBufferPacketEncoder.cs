using System.Buffers;
using System.Collections.Concurrent;
using Common.ObjectPoolPolicy;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.ObjectPool;
using Protocol.Protobuf;

namespace Common.Encoder;

public class ProtocolBufferPacketEncoder : MessageToByteEncoder<IMessage>
{
    private static readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;
    private static readonly ConcurrentDictionary<MessageDescriptor, FieldDescriptor> FieldDescriptorCache = new(Packet.Descriptor.Fields.InDeclarationOrder().ToDictionary(x => x.MessageType, x => x));
    private static readonly ObjectPool<Packet> PacketPool = ObjectPool.Create(new ProtocolBufferPacketObjectPoolPolicy());

    protected override void Encode(IChannelHandlerContext context, IMessage message, IByteBuffer output)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!FieldDescriptorCache.TryGetValue(message.Descriptor, out var field))
        {
            throw new InvalidOperationException("field not found");
        }

        var packet = PacketPool.Get();
        try
        {
            field.Accessor.SetValue(packet, message);
            var length = packet.CalculateSize();
        
            using var memoryOwner = MemoryPool.Rent(length);
            var memory = memoryOwner.Memory;
            packet.WriteTo(memory.Span[..length]);
            output.EnsureWritable(length);
        
            if (output.HasArray)
            {
                var startIndex = output.ArrayOffset + output.WriterIndex;
                memory.Span[..length].CopyTo(output.Array.AsSpan(startIndex, length));
                output.SetWriterIndex(output.WriterIndex + length);
            }
            else
            {
                output.WriteBytes(memory.Span[..length].ToArray());
            }
        }
        finally
        {
            PacketPool.Return(packet);
        }
    }
    
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        context.FireExceptionCaught(exception);
    }
}