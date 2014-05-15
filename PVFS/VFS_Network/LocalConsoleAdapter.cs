using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VFS.VFS;
using VFS_GUI;

namespace VFS_Network
{
    /// <summary>
    /// An instance of this calss is run by the Client.
    /// </summary>
    class LocalConsoleAdapter : LocalConsole
    {
        private const int BUFFER_SIZE = 4096;

        private bool abort;
        private object abortLock;
        private VfsClient clientGUI;

        private TcpClient client;
        private Thread clientThread;

        private Dictionary<string, List<string>> _userToSequenceNumber = new Dictionary<string, List<string>>();  

        public LocalConsoleAdapter(RemoteConsole remc, VfsClient clientGUI)
            : base(remc)
        {
            abortLock = new Object();
            this.clientGUI = clientGUI;

            string path = Environment.CurrentDirectory + "\\sequence.bin";
            if (File.Exists(path))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                _userToSequenceNumber = (Dictionary<string, List<string>>)formatter.Deserialize(stream);
                stream.Close();
            }
            else
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, _userToSequenceNumber);
                stream.Close();
            }

        }

        //-------------------Public-------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns>0 -> ok, 1 -> connection error, 2-> wrong user/pass</returns>
        public int Start(string IP, int port, string user, string password)
        {
            bool error = true;
            try
            {
                client = new TcpClient(IP, port);

                Byte[] data = new byte[1 + user.Length + 32];

                data[0] = (Byte)user.Length;

                Encoding.UTF8.GetBytes(user, 0, user.Length, data, 1);

                VfsServer.GetHash(password).CopyTo(data, user.Length + 1);

                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                int i = stream.Read(data, 0, 1);

                if (i != 1)//connection error
                    return 1;

                if (data[0] != 1)//wrong user/password
                    return 2;

                lock (abortLock)
                {
                    this.abort = false;
                }
                stream.ReadTimeout = 500;
                this.clientThread = new Thread(this.clientProcedure);
                this.clientThread.Name = "VFS Client";
                this.clientThread.Start();

                error = false;
            }
            catch (SocketException e)
            {
                return 1;
            }
            finally
            {
                if (error && client != null)
                {
                    client.Close();
                    client = null;
                }
            }
            
            return 0;
        }
        
        public void Stop()
        {
            lock (abortLock)
            {
                this.abort = true;
            }

            string path = Environment.CurrentDirectory + "\\sequence.bin";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, _userToSequenceNumber);
            stream.Close();
        }

        //-------------------Private-------------------

        private void clientProcedure()
        {
            Byte[] buffer = new Byte[BUFFER_SIZE];
            NetworkStream stream = client.GetStream();

            while (true)
            {
                lock (abortLock)
                {
                    if (this.abort)
                        break;
                }

                Thread.Sleep(10);

                if (!client.Connected)
                    break;

                try
                {
                    int length = stream.Read(buffer, 0, buffer.Length);

                    if (length > 0)
                    {
                        if (!handleData(buffer, length))
                            break;
                    }
                }
                catch (IOException) { }
            }

            // Send end
            RemoteConsoleAdapter.SendData(client, new Byte[] { 8 });

            this.client.Close();
            this.client = null;

            clientGUI.Invoke(new Action(clientGUI.Disconnect));
        }

        private bool handleData(Byte[] data, int length)
        {
            switch (data[0])
            {
                case 0:// Wrong user
                    return false;
                case 1:// Accept User
                    break;
                case 2:
                    var commLength = BitConverter.ToInt32(data, 1);

                    var comm = Encoding.UTF8.GetString(data, 5, commLength);
                    var seq = BitConverter.ToInt32(data, 5 + commLength);

                    if (seq > _userToSequenceNumber[clientGUI.Username].Count + 1)
                    {
                        //TODO: fetch missing commands
                    }
                    if (seq == _userToSequenceNumber[clientGUI.Username].Count + 1)
                    {
                        _userToSequenceNumber[clientGUI.Username].Add(comm);
                    }
                    if (seq < _userToSequenceNumber[clientGUI.Username].Count + 1)
                    {
                        //TODO: sync back
                    }

                    VfsManager.ExecuteCommand(comm);
                    break;
                case 3:// Message
                    if (length >= 2)
                    {
                        int commandLength = BitConverter.ToInt32(data, 1);
                        int messageLength = BitConverter.ToInt32(data, 5);
                        if (commandLength < 0 || messageLength < 0) return true;
                        string command = Encoding.UTF8.GetString(data, 9, commandLength);
                        string message = Encoding.UTF8.GetString(data, 9 + commandLength, messageLength);

                        remote.Message(command, message, null);
                    }
                    break;
                case 4:// Error
                    if (length >= 2)
                    {
                        int commandLength = BitConverter.ToInt32(data, 1);
                        int messageLength = BitConverter.ToInt32(data, 5);
                        if (commandLength < 0 || messageLength < 0) return true;
                        string command = Encoding.UTF8.GetString(data, 9, commandLength);
                        string message = Encoding.UTF8.GetString(data, 9 + commandLength, messageLength);

                        remote.ErrorMessage(command, message, null);
                    }
                    break;
                case 5:// Query
                    if (length >= 2)
                    {
                        int commandLength = BitConverter.ToInt32(data, 1);
                        if (commandLength < 0) return true;
                        string command = Encoding.UTF8.GetString(data, 5, commandLength);

                        clientGUI.Invoke(new Action(() => handleQuery(command)));
                    }
                    break;
                case 6:// File Transfer im
                    // TODO TODO TODO
                    break;
                case 7:// File Transfer ex
                    // TODO TODO TODO
                    break;
                case 8:// End
                    return false;
            }
            return true;
        }

        private void handleQuery(string message)
        {
            int res = remote.Query(message, null, null);

            RemoteConsoleAdapter.SendData(client, new Byte[] { 5, (Byte)res });
        }

        //-------------------Interface-------------------

        public override void Command(string comm, VFS.VFS.OnlineUser sender)
        {
            if (this.client != null && client.Connected)
            {
                if (comm == "quit")
                    return;
            
                Byte[] data = new Byte[1 + 4 + comm.Length];

                data[0] = 2;

                BitConverter.GetBytes(comm.Length).CopyTo(data, 1);

                Encoding.UTF8.GetBytes(comm, 0, comm.Length, data, 5);

                RemoteConsoleAdapter.SendData(client, data);
            }
        }

        public override void Message(string info)
        {
            
        }
        public override void ErrorMessage(string message)
        {

        }
        public override int Query(string message, params string[] options)
        {
            return 0;
        }
    }
}
