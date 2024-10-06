using System.Net;
using Common.Adapter;
using Common.Codec;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NettyServer.Adapter;
using NettyServer.InboundHandler;
using Protocol.MemoryPack;

var bossGroup = new MultithreadEventLoopGroup(1);
var workerGroup = new MultithreadEventLoopGroup();

try
{
#if MEMORYPACK
    PacketManager.RegisterPacket();
#endif // MEMORYPACK
    
    var bootstrap = new ServerBootstrap();
    bootstrap.Group(bossGroup, workerGroup)
        .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
        .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
        .Channel<TcpServerSocketChannel>()
        .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
        {
            var pipeline = channel.Pipeline;

            // packet size limit
            pipeline.AddLast(new LengthFieldBasedFrameDecoder(
                maxFrameLength: 1024 * 1024,
                lengthFieldOffset: 0,
                lengthFieldLength: 4,
                lengthAdjustment: 0,
                initialBytesToStrip: 4
            ));

            // adapter
            pipeline.AddFirst(new ConnectionLogAdapter());
            pipeline.AddFirst(new ExceptionLogAdapter());
            pipeline.AddFirst(new ExceptionCloseAdapter());
            pipeline.AddFirst(new ReadCompleteFlushAdapter());

#if PROTOBUF
            pipeline.AddLast(new DotNetty.Codecs.Protobuf.ProtobufVarint32FrameDecoder());
            pipeline.AddLast(new DotNetty.Codecs.Protobuf.ProtobufVarint32LengthFieldPrepender());
            pipeline.AddLast(new ProtobufPacketCodec());
            pipeline.AddLast(new ProtobufMessageHandler());
#endif // PROTOBUF

#if MEMORYPACK
            pipeline.AddLast(new MemoryPackPacketCodec());
            pipeline.AddLast(new MemoryPackMessageHandler());
#endif // MEMORYPACK
            
#if MESSAGEPACK
            pipeline.AddLast(new MessagePackPacketCodec());
            pipeline.AddLast(new MessagePackMessageHandler());
#endif // MEMORYPACK
        }));

    var channel = await bootstrap.BindAsync(new IPEndPoint(IPAddress.Any, 20000));
    Console.WriteLine("Server started on port 20000");

    await Task.Delay(-1);
    await channel.CloseAsync();
}
finally
{
    Console.WriteLine("Finally");
    await bossGroup.ShutdownGracefullyAsync();
    await workerGroup.ShutdownGracefullyAsync();
}