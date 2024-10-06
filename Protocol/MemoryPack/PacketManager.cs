using MemoryPack;
using MemoryPack.Formatters;
using Protocol.MemoryPack.Packet;

namespace Protocol.MemoryPack;

public static class PacketManager
{
    public static void RegisterPacket()
    {
        var formatter = new DynamicUnionFormatter<IPacket>(
            (0, typeof(PingPacket)),
            (1, typeof(PongPacket))
        );
        
        MemoryPackFormatterProvider.Register(formatter);
    }
}