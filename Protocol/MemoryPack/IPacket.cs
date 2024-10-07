using MemoryPack;
using Protocol.MemoryPack.Packet;

namespace Protocol.MemoryPack;

[MemoryPackable]
[MemoryPackUnion(0, typeof(PingPacket))]
[MemoryPackUnion(1, typeof(PongPacket))]
public partial interface IPacket
{
}