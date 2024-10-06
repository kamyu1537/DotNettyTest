using MemoryPack;

namespace Protocol.MemoryPack.Packet;

[MemoryPackable]
public partial class PongPacket : IPacket
{
    public string ChannelId { get; set; } = null!;
    public int Data { get; set; }
    public DateTime Time { get; set; }
}