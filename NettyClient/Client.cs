using System.Net;
using Common.Adapter;
using Common.Codec;
using DotNetty.Codecs;
using DotNetty.Codecs.Protobuf;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NettyClient.Adapter;
using NettyClient.InboundHandler;

namespace NettyClient;

public class Client(EndPoint endpoint)
{
    public async ValueTask RunAsync(CancellationToken cancel)
    {
        var group = new MultithreadEventLoopGroup(1);

        try
        {
            var bootstrap = new Bootstrap();
            bootstrap.Group(group)
                .Channel<TcpSocketChannel>()
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    // packet size limit
                    pipeline.AddLast(new LengthFieldPrepender(4));

                    // adapter
                    pipeline.AddFirst(new ConnectionLogAdapter());
                    pipeline.AddFirst(new ExceptionLogAdapter());
                    pipeline.AddFirst(new ReadCompleteFlushAdapter());
                    
#if PROTOBUF
                    pipeline.AddLast(new ProtobufVarint32FrameDecoder());
                    pipeline.AddLast(new ProtobufVarint32LengthFieldPrepender());
                    pipeline.AddLast(new ProtobufPacketCodec());
                    pipeline.AddLast(new ProtobufMessageHandler());
                    
                    pipeline.AddLast(new SendProtobufPingAdapter());
#endif // PROTOBUF

#if MEMORYPACK
                    pipeline.AddLast(new MemoryPackPacketCodec());
                    pipeline.AddLast(new MemoryPackMessageHandler());
                    
                    pipeline.AddLast(new SendMemoryPackPingAdapter());
#endif // MEMORYPACK
                    
#if MESSAGEPACK
                    pipeline.AddLast(new MessagePackPacketCodec());
                    pipeline.AddLast(new MessagePackMessageHandler());

                    pipeline.AddLast(new SendMessagePackPingAdapter());
#endif // MEMORYPACK
                }));

            var channel = await bootstrap.ConnectAsync(endpoint);

            while (channel.Active)
            {
                if (cancel.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(1000, cancel);
            }

            Console.WriteLine("Close: " + channel.Id);
            await channel.CloseAsync();
        }
        finally
        {
            await group.ShutdownGracefullyAsync();
        }
    }
}