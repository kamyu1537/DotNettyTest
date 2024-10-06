using MessagePack;
using Protocol.MessagePack.Packet;

namespace Protocol.MessagePack;


[Union(0, typeof(PingPacket))]
[Union(1, typeof(PongPacket))]
public interface IPacket;