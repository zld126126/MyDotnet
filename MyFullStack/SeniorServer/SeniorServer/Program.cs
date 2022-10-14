using System;
using System.Collections.Generic;
using System.Threading;

namespace SeniorServer
{
    class Program
    {
        public static TcpServer.NettyServer S_NettyServer = new TcpServer.NettyServer();
        static void Main(string[] args)
        {
            //启动netty
            Thread nettyThread = new Thread(() =>
            {
                S_NettyServer.RunServerAsync().Wait();
            });
            nettyThread.Start();
            Debug.Instance.Log("Netty启动完成");
        }
    }
}
