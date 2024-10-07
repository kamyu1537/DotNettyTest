using Microsoft.Extensions.ObjectPool;
using Protocol.Protobuf;

namespace Common.ObjectPoolPolicy;

public class ProtocolBufferPacketObjectPoolPolicy : PooledObjectPolicy<Packet>
{
    public override Packet Create()
    {
        return new Packet();
    }

    public override bool Return(Packet obj)
    {
        obj.ClearData();
        return true;
    }
}