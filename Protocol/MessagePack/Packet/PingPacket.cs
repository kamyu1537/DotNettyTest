using MessagePack;

namespace Protocol.MessagePack.Packet;

[MessagePackObject]
public class PingPacket : IPacket
{
    [Key(0)]
    public string ChannelId { get; set; } = null!;
    
    [Key(1)]
    public int Data { get; set; }
    
    [Key(2)]
    public DateTime Time { get; set; }
}