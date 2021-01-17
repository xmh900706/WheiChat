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
using System.Data.SqlClient;

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
                    StateObject1 stateObject = new StateObject1();
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

            StateObject1 state = obj as StateObject1;
            try
            {
                int lenght = -1;
                lenght = state.client.Receive(state.buffer);
                AccountTemplate template = new AccountTemplate();
                template = Deserialize(state.buffer);
                int i = QueryTheDatabase(template);
                byte[] vs = new byte[1];
                vs[0] = Convert.ToByte(i);
                state.client.BeginSend(vs, 0, 1, SocketFlags.None, new AsyncCallback(SendData), state);
            }
            catch
            {
                return;
            }
        }

        private void SendData(IAsyncResult ar)
        {
            try
            {
                StateObject1 state = (StateObject1)ar.AsyncState;
                state.client.EndSend(ar);
            }
            catch
            {
                return;
            }
        }


        ///<summary>
        /// 序列化
        /// </summary>
        /// <param name="data">要序列化的对象</param>
        /// <returns>返回存放序列化后的数据缓冲区</returns>
        private static byte[] Serialize(object data)
        {

            //序列化并写入内存流
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, data);
            byte[] byteMsg = new byte[memoryStream.Length];
            byteMsg = memoryStream.GetBuffer();
            return byteMsg;

        }


        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static AccountTemplate Deserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            BinaryFormatter b = new BinaryFormatter();
            Object objectTry = b.Deserialize(ms);
            AccountTemplate template = objectTry as AccountTemplate;
            return template;
        }


        private int QueryTheDatabase(AccountTemplate account)
        {
            try
            {
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=.\UserDatabase.mdf;Integrated Security=True";
                string commandText = "AccountVerif";
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = commandText;
                sqlCommand.Parameters.Add(new SqlParameter("@account", System.Data.SqlDbType.VarChar, 50));
                sqlCommand.Parameters.Add(new SqlParameter("@password", System.Data.SqlDbType.VarChar, 50));
                sqlCommand.Connection.Open();
                int i = sqlCommand.ExecuteNonQuery();
                return i;
            }
            catch
            {
                return -1;
            }
        }
    }
    class StateObject1
    {
        public Socket client;
        public const int bufferSize = 256;
        public byte[] buffer = new byte[bufferSize];

    }
}
