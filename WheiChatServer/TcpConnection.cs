using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WheiChatServer
{
    class TcpConnection
    {
        /// <summary>
        /// 构造函数 传入监听端口
        /// </summary>
        /// <param name="port"></param>
        public TcpConnection(int port)
        {
            _port = port;
        }


        //监听端口
        private int _port;

        ManualResetEvent manual = new ManualResetEvent(false);

        public void StartListen()
        {
            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");  //本机IP地址
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, _port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(iPEndPoint);  //绑定端口
                listener.Listen(10);
                while(true)
                {
                    manual.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener); //异步接受客户端连接。
                    manual.WaitOne();  //阻塞循环 直到有客户端接入。
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 异步接受客户端回调函数 
        /// 
        /// 接受客户端消息
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            manual.Set();
            try
            {
                Socket client;
                client= listener.EndAccept(ar);  
                Structure clientStructure = new Structure();   //客户端结构体
                clientStructure.socket = client;

                //异步接受客户端消息
                client.BeginReceive(clientStructure.buffer, 0, Structure.bufferSize, 0, new AsyncCallback(ReceiveDataCallback), clientStructure);
            }
            catch
            {
                return;
            }
        }


        /// <summary>
        /// 异步接收客户端消息回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveDataCallback(IAsyncResult ar)
        {
            try
            {
                Structure structureClient = (Structure)ar.AsyncState;
                structureClient.socket.EndReceive(ar);
                
            }
            catch
            {
                return;
            }
        }
    }
    class Structure
    {
        public Socket socket;
        public const int bufferSize = 256;
        public byte[] buffer = new byte[bufferSize];
    }
}
