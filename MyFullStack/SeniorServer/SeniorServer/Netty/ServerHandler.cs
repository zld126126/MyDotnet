namespace TcpServer
{
    using System;
    using System.Linq;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    public class EchoServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelActive(IChannelHandlerContext context)
        {

        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                Console.WriteLine("Received from client: " + buffer.ToString(Encoding.UTF8));
            }

            ReceiveFromClient(context, buffer);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            context.CloseAsync();
        }
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
           //  => context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }

        /// <summary>
        /// 发送客户端string
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        void SendToClient(IChannelHandlerContext context,string msg) {
            var buffer = ByteBufferUtil.EncodeString(ByteBufferUtil.DefaultAllocator,msg,Encoding.UTF8);
            if (buffer != null) {
                context.Channel.WriteAndFlushAsync(buffer);
                Debug.Instance.LogError("发送消息:" + msg.ToString());
            }
        }

        /// <summary>
        /// 发送消息 MsgTest data = new MsgTest();data.RecContent = "这是1个Server回复";
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        void SendToClient(IChannelHandlerContext context, MsgBase msg)
        {
            byte[] nameBytes = MsgBase.EncodeName(msg);
            byte[] bodyBytes = MsgBase.Encode(msg);
            byte[] dataBuff = nameBytes.Concat(bodyBytes).ToArray();

            IByteBuffer buffer = Unpooled.WrappedBuffer(dataBuff);
            context.Channel.WriteAndFlushAsync(buffer);
            Debug.Instance.LogError("发送消息:"+msg.ToString());
        }

        void ReceiveFromClient(IChannelHandlerContext context, IByteBuffer buffer) {
            byte[] bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);

            int nameCount = 0;
            int readIdx = 0;
            ProtocolEnum protocol = MsgBase.DecodeName(bytes, readIdx, out nameCount);
            if (protocol == ProtocolEnum.None)
            {
                string s = buffer.ToString(Encoding.UTF8);
                Debug.Instance.LogError("收到消息:"+s);
                Debug.Instance.LogError("ReceiveFromClient DecodeName Fail");
                context.CloseAsync();
                return;
            }

            readIdx += nameCount;
            MsgBase msgBase = MsgBase.Decode(protocol, bytes, readIdx, bytes.Length - readIdx);
            if (msgBase == null)
            {
                Debug.Instance.LogError("ReceiveFromClient Decode Fail");
                context.CloseAsync();
                return;
            }

            if (msgBase is MsgCSharp)
            {
                var msg = msgBase as MsgCSharp;
                msg.RecContent = "我是3!-server";
                SendToClient(context, msgBase);
            }
            else if (msgBase is MsgLua)
            {
                var msg = msgBase as MsgLua;
                Debug.Instance.Log(msg.LuaJson);
            }
            else { 
                // TODO 其他类型支持
            }
        }
    }
}