using System;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace TcpServer
{
    public class NettyServer 
    {
        private int mBlocking = 1; 
        public NettyServer() { }
        public  async Task RunServerAsync()
        {
            IEventLoopGroup bossGroup;
            IEventLoopGroup workerGroup;

            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup(4);
            try
            {
                var bootstrap = new ServerBootstrap();

                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<TcpServerSocketChannel>();
                bootstrap.Option(ChannelOption.SoBacklog, 1024)
                .Option(ChannelOption.SoReuseport, true)
                .Option(ChannelOption.Allocator, new PooledByteBufferAllocator())
                .ChildOption(ChannelOption.Allocator, new PooledByteBufferAllocator())
                .Handler(new LoggingHandler("SRV-LSTN"))
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>//��ʼ��Tcp����
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                    //ByteOrder.LittleEndian 统一unity和netty的大小端问题...
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(ByteOrder.LittleEndian, 2,0,false));//����Ȼ����
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ByteOrder.LittleEndian, ushort.MaxValue, 0, 2, 0, 2, false));//����
                    pipeline.AddLast("echo", new EchoServerHandler());
                }
                ));
                // ServerSettings.Port
                IChannel boundChannel = await bootstrap.BindAsync(10188);

                while (mBlocking > 0) { };
                await boundChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        public void Stop()
        {
            System.Threading.Interlocked.Decrement(ref mBlocking);
        }
    }
}
