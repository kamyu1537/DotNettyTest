using MemoryPack;

namespace Protocol.MemoryPack.Packet;

[MemoryPackable]
public partial class PingPacket : IPacket
{
    public string ChannelId { get; set; } = null!;
    public string Message { get; set; } = null!;
    public int Data { get; set; }
    public DateTime Time { get; set; }
}