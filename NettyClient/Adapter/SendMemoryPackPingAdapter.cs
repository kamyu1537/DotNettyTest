﻿using Common.Utils;
using DotNetty.Transport.Channels;
using Protocol.MemoryPack.Packet;

namespace NettyClient.Adapter;

public class SendMemoryPackPingAdapter : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        base.ChannelActive(context);

        context.WriteAndFlushAsync(new PingPacket
        {
            ChannelId = context.Channel.Id.AsLongText(),
            Message = RandomString.Generate(1024),
            Data = Random.Shared.Next(),
            Time = DateTime.UtcNow
        });
        
        Console.WriteLine($"MemoryPack Send: {context.Channel.Id}");
    }
}