using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Template;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WheiChatServer
{
    class AccessVerifyServer
    {
        public void StartServer()
        {
            
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(iPEndPoint);
                listener.Listen(10);
                Task acceptTask = new Task(new Action<object>( AcceptClient), listener);
                acceptTask.Start();
            }
            catch(Exception e)
            {
                return;
            }
        }

        ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private void AcceptClient(object objListener)
        {
            Socket listener = objListener as Socket;
            while(true)
            {
                try
                {
                    Socket socketClient = listener.Accept();
                    StateObject stateObject = new StateObject();
                    stateObject.client = socketClient;
                    Task clientTask = new Task(new Action<object>(ReceiveData), stateObject);
                    clientTask.Start();
                }
                catch(Exception e)
                {
                    break;
                }
            }
        }

        private void ReceiveData(object obj)
        {

            StateObject state = obj as StateObject;
            try
            {
                int lenght = -1;
                lenght = state.client.Receive(state.buffer);
                AccountTemplate template = new AccountTemplate();
                template = Deserialize(state.buffer);
            }
            catch
            {
                return;
            }
        }
        public static AccountTemplate Deserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            BinaryFormatter b = new BinaryFormatter();
            Object objectTry = b.Deserialize(ms);
            AccountTemplate template = objectTry as AccountTemplate;
            return template;
        }
    }
    class StateObject
    {
        public Socket client;
        public const int bufferSize = 256;
        public byte[] buffer = new byte[bufferSize];

    }
}
