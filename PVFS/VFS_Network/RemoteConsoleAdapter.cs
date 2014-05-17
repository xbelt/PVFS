using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VFS_GUI;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using VFS.VFS;
using System.IO;

namespace VFS_Network
{
    /// <summary>
    /// An instance of this calss is run by the Server.
    /// </summary>
    class RemoteConsoleAdapter : RemoteConsole
    {
        private const int BUFFER_SIZE = 4096;

        private VfsServer serverGUI;
        private bool ready, abort;
        private int queryResult;
        private object queryObject;

        List<OnlineUser> onlineUsers;
        TcpListener serverSocket;
        Thread serverThread;
        private Dictionary<string, List<string>> _userToSequenceNumber = new Dictionary<string, List<string>>();  

        public RemoteConsoleAdapter(VfsServer serverGUI)
        {
            this.serverGUI = serverGUI;
            ready = true;
            this.queryObject = new Object();
            this.onlineUsers = new List<OnlineUser>();

            string path = Environment.CurrentDirectory + "\\sequence.bin";
            if (File.Exists(path))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                _userToSequenceNumber = (Dictionary<string, List<string>>) formatter.Deserialize(stream);
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

        public void Start(int port)
        {
            serverSocket = new TcpListener(IPAddress.Any, port);
            serverThread = new Thread(this.serverProcedure);
            serverThread.Name = "VFS Server";
            serverThread.Start();
        }

        public void Stop()
        {
            lock (this)
            {
                this.abort = true;
            }
            string path = Environment.CurrentDirectory + "\\sequence.bin";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, _userToSequenceNumber);
            stream.Close();
        }

        public static void SendData(TcpClient client, Byte[] data)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
                catch (ObjectDisposedException) { }
                catch (IOException) { }
            }
        }

        //-------------------Private-------------------

        private void serverProcedure()
        {
            try
            {
                serverSocket.Start();
            }
            catch (SocketException)
            {
                serverGUI.InvokeLog("Failed Serverstart-----------------");
                serverGUI.Invoke(new Action(() =>
                {
                    serverGUI.startServerButton_Click(null, null);
                    serverGUI.EnableStartServer();
                }));
                return;
            }

            lock (this)
            {
                this.abort = false;
            }

            serverGUI.InvokeLog("Started Server---------------------");

            while (true)
            {
                while (!serverSocket.Pending())
                {
                    lock (this)
                    {
                        if (this.abort)
                            goto break2;
                    }
                    Thread.Sleep(166);
                }

                TcpClient client = serverSocket.AcceptTcpClient();

                startClient(client);
            }
            break2:

            serverSocket.Stop();
            serverGUI.Invoke(new Action(serverGUI.EnableStartServer));
            serverGUI.InvokeLog("Stopped Server---------------------");
        }

        private void startClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            Byte[] data = new Byte[200];

            while (stream.Read(data, 0, 200) == 0)
            {
                Thread.Sleep(100);
            }

            int nameLength = data[0];

            string name = Encoding.UTF8.GetString(data, 1, nameLength);

            VfsUser user = serverGUI.Users.FirstOrDefault(u => u.Name == name);
            if (user == null || !data.EqualContent(1 + nameLength, 32, user.PasswordHash))
            {
                serverGUI.InvokeLog(">> Rejected connection atempt" + (user != null ? " from " + user.Name : "") + ".");

                stream.Write(new Byte[] { 0 }, 0, 1);
                stream.Flush();
                client.Close();
            }
            else
            {
                serverGUI.InvokeLog(">> Accepted user " + user.Name + ".");

                stream.Write(new Byte[] { 1 }, 0, 1);
                stream.Flush();

                user.Online = true;
                serverGUI.Invoke(new Action(() => serverGUI.SetOnlineState(user)));

                if (onlineUsers.Any(x => x.Name == user.Name))
                {
                    onlineUsers.First(x => x.Name == user.Name).Connection.Add(client);
                }
                else
                {
                    var onuser = new OnlineUser { Name = user.Name };
                    onuser.Connection = new List<TcpClient>();
                    onuser.Connection.Add(client);
                    onlineUsers.Add(onuser);
                    new Thread(this.clientProcedure).Start(onuser);
                }

            }
        }

        private void clientProcedure(object client)
        {
            var user = (OnlineUser)client;
            var streams = user.Connection.Select(x => x.GetStream()).ToList();
            var buffer = new Byte[BUFFER_SIZE];
            streams.ForEach(x => x.ReadTimeout = 500);

            while (true)
            {
                lock (this)
                {
                    if (abort)
                    {
                        foreach (var stream in streams)
                        {
                            stream.Write(new Byte[] { 8 }, 0, 1);
                            stream.Flush();
                        }
                    }
                }

                if (user.Connection.Any(x => !x.Connected))
                {
                    serverGUI.InvokeLog(">> Disconnected user " + user.Name + ".");

                    lock (this.queryObject)
                    {
                        this.queryResult = 1;
                        Monitor.PulseAll(this.queryObject);
                    }

                    var conn = user.Connection.First(x => !x.Connected);
                    conn.Close();
                    user.Connection.Remove(conn);
                    if (!user.Connection.Any())
                    {
                        var u = serverGUI.Users.First(e => e.Name == user.Name);
                        u.Online = false;
                        if (serverGUI.ready)
                            serverGUI.Invoke(new Action(() => serverGUI.SetOnlineState(u)));
                        return;
                    }
                }

                Thread.Sleep(10);

                try
                {
                    var count = 0;
                    foreach (var i in streams.Select(stream => stream.Read(buffer, 0, buffer.Length)))
                    {
                        if (i > 0)
                        {
                            if (!handleData(user, buffer, i, count))
                            {
                                break;
                            }
                        }
                        count ++;
                    }
                }
                catch (IOException) { }
            }
        }

        private bool handleData(OnlineUser user, byte[] data, int length, int count)
        {
            switch (data[0])
            {
                case 0:// Wrong User
                    break;
                case 1:// Accept User
                    break;
                case 2:// Command
                    var commLength = BitConverter.ToInt32(data, 1);

                    var comm = Encoding.UTF8.GetString(data, 5, commLength);

                    if (comm.StartsWith("fetch"))
                    {
                        serverGUI.InvokeLog(user.Name + " -> " + comm);

                        var seqNumber = Convert.ToInt32(comm.Substring(5));

                        lock (_userToSequenceNumber)
                        {
                            for (int i = seqNumber; i < _userToSequenceNumber[user.Name].Count; i++)
                            {
                                var command = "fetch " + _userToSequenceNumber[user.Name][i];
                                var newData = new Byte[1 + 4 + command.Length + 4];
                                newData[0] = 2;
                                BitConverter.GetBytes(command.Length).CopyTo(newData, 1);
                                Encoding.UTF8.GetBytes(command, 0, command.Length, newData, 5);
                                BitConverter.GetBytes(i + 1)
                                    .CopyTo(newData, 5 + command.Length);
                                SendData(user.Connection[count], newData);
                            }
                        }
                        return true;
                    }

                    if (comm.StartsWith("sync"))
                    {
                        serverGUI.InvokeLog(user.Name + " -> " + comm);

                        comm = comm.Substring(4);

                        if (!_userToSequenceNumber.ContainsKey(user.Name))
                        {
                            _userToSequenceNumber.Add(user.Name, new List<string>());
                        }
                        _userToSequenceNumber[user.Name].Add(comm);

                        if (comm == "quit" || comm == "q" || comm == "exit")
                            return true; // Ignore quit command!
                        else
                        {
                            if (comm.StartsWith("im"))
                            {

                            }
                            else
                            {
                                var newData = new Byte[1 + 4 + comm.Length + 4];
                                newData[0] = 2;
                                BitConverter.GetBytes(comm.Length).CopyTo(newData, 1);
                                Encoding.UTF8.GetBytes(comm, 0, comm.Length, newData, 5);
                                BitConverter.GetBytes(_userToSequenceNumber[user.Name].Count)
                                    .CopyTo(newData, 5 + comm.Length);
                                var index = 0;
                                foreach (var client in user.Connection)
                                {
                                    if (count != index)
                                        SendData(client, newData);
                                    index++;
                                }
                            }
                            //TODO if import -> manage file transfer (haha)

                            //TODO if export -> modify command to export to a temp local dir

                            local.Command(comm, user);
                        }

                        return true;
                    }

                    lock (_userToSequenceNumber)
                    {
                        if (!_userToSequenceNumber.ContainsKey(user.Name))
                        {
                            _userToSequenceNumber.Add(user.Name, new List<string>());
                        }
                        _userToSequenceNumber[user.Name].Add(comm);

                        serverGUI.InvokeLog(user.Name + " -> " + comm);

                        if (comm == "quit" || comm == "q" || comm == "exit")
                            return true; // Ignore quit command!
                        else
                        {
                            if (comm.StartsWith("im"))
                            {

                            }
                            else
                            {
                                var newData = new Byte[1 + 4 + comm.Length + 4];
                                newData[0] = 2;
                                BitConverter.GetBytes(comm.Length).CopyTo(newData, 1);
                                Encoding.UTF8.GetBytes(comm, 0, comm.Length, newData, 5);
                                BitConverter.GetBytes(_userToSequenceNumber[user.Name].Count)
                                    .CopyTo(newData, 5 + comm.Length);
                                foreach (var client in user.Connection)
                                {
                                    SendData(client, newData);
                                }
                            }
                            //TODO if import -> manage file transfer (haha)

                            //TODO if export -> modify command to export to a temp local dir

                            local.Command(comm, user);
                        }
                    }
                    break;
                case 3:// Message
                    break;
                case 4:// Error
                    break;
                case 5:// Query (result)
                    serverGUI.InvokeLog(user.Name + " -> Query result: " + data[1]);
                    lock (this.queryObject)
                    {
                        this.queryResult = data[1];
                        Monitor.PulseAll(this.queryObject);
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
                default:
                    break;
            }
            return true;
        }

        //-------------------Interface-------------------

        public override void Command(string comm) { }

        public override void Message(string command, string info, OnlineUser sender)
        {
            if (sender.Connection != null && sender.Connection.Any(x => x.Connected))
            {
                //TODO if export -> manage file transfer (haha)


                serverGUI.InvokeLog(sender.Name + " <- " + info);

                Byte[] data = new Byte[1 + 4 + 4 + command.Length + info.Length];

                data[0] = 3;

                BitConverter.GetBytes(command.Length).CopyTo(data, 1);
                BitConverter.GetBytes(info.Length).CopyTo(data, 5);

                Encoding.UTF8.GetBytes(command, 0, command.Length, data, 9);
                Encoding.UTF8.GetBytes(info, 0, info.Length, data, 9 + command.Length);
                foreach (var client in sender.Connection)
                {
                    SendData(client, data);
                }
            }
        }

        public override void ErrorMessage(string command, string message, OnlineUser sender)
        {
            if (sender.Connection != null && sender.Connection.Any(x => x.Connected))
            {
                serverGUI.InvokeLog(sender.Name + " <- " + message);

                Byte[] data = new Byte[1 + 4 + 4 + command.Length + message.Length];

                data[0] = 4;

                BitConverter.GetBytes(command.Length).CopyTo(data, 1);
                BitConverter.GetBytes(message.Length).CopyTo(data, 5);

                Encoding.UTF8.GetBytes(command, 0, command.Length, data, 9);
                Encoding.UTF8.GetBytes(message, 0, message.Length, data, 9 + command.Length);

                foreach (var client in sender.Connection)
                {
                    SendData(client, data);
                }
            }
        }

        public override int Query(string message, string[] options, OnlineUser sender)
        {
            if (sender.Connection != null && sender.Connection.Any(x => x.Connected))
            {
                serverGUI.InvokeLog(sender.Name + " <- Query: " + message);

                Byte[] data = new Byte[1 + 4 + message.Length];

                data[0] = 5;

                BitConverter.GetBytes(message.Length).CopyTo(data, 1);

                Encoding.UTF8.GetBytes(message, 0, message.Length, data, 5);

                foreach (var client in sender.Connection)
                {
                    SendData(client, data);
                }

                lock (this.queryObject)
                {
                    Monitor.Wait(this.queryObject);
                    return this.queryResult;
                }
            }
            else return 1;
        }

        public override void SetBusy()
        {
            ready = false;
        }

        public override void SetReady()
        {
            ready = true;
        }

        public override void setExplorer(VfsExplorer explorer)
        {
            throw new InvalidOperationException();
        }
    }
}
