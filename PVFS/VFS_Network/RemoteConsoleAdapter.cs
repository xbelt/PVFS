using System;
using System.Collections.Generic;
using System.Linq;
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

        public RemoteConsoleAdapter(VfsServer serverGUI)
        {
            this.serverGUI = serverGUI;
            ready = true;
            this.queryObject = new Object();
            this.onlineUsers = new List<OnlineUser>();
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
            if (user == null || !data.EqualContent(1 + nameLength, 32, user.PasswordHash) || user.Online)
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

                OnlineUser onuser = new OnlineUser() { Connection = client, Name = user.Name };

                this.onlineUsers.Add(onuser);
                new Thread(this.clientProcedure).Start(onuser);
            }
        }

        private void clientProcedure(object client)
        {
            OnlineUser user = (OnlineUser)client;
            NetworkStream stream = user.Connection.GetStream();
            Byte[] buffer = new Byte[BUFFER_SIZE];
            stream.ReadTimeout = 500;

            while (true)
            {
                lock (this)
                {
                    if (this.abort)
                    {
                        stream.Write(new Byte[] { 8 }, 0, 1);
                        stream.Flush();
                    }
                }

                if (!user.Connection.Connected)
                    break;

                Thread.Sleep(10);

                try
                {
                    int i = stream.Read(buffer, 0, buffer.Length);

                    if (i > 0)
                    {
                        if (!handleData(user, buffer, i))
                            break;
                    }
                }
                catch (IOException) { }
            }

            serverGUI.InvokeLog(">> Disconnected user " + user.Name + ".");

            lock (this.queryObject)
            {
                this.queryResult = 1;
                Monitor.PulseAll(this.queryObject);
            }

            VfsUser u = serverGUI.Users.First(e => e.Name == user.Name);
            u.Online = false;
            if (serverGUI.ready)
                serverGUI.Invoke(new Action(() => serverGUI.SetOnlineState(u)));

            user.Connection.Close();
        }

        private bool handleData(OnlineUser user, Byte[] data, int length)
        {
            switch (data[0])
            {
                case 0:// Wrong User
                    break;
                case 1:// Accept User
                    break;
                case 2:// Command
                    int commLength = BitConverter.ToInt32(data, 1);

                    string comm = System.Text.Encoding.UTF8.GetString(data, 5, commLength);

                    serverGUI.InvokeLog(user.Name + " -> " + comm);

                    if (comm == "quit" || comm == "q" || comm == "exit")
                        return true;// Ignore quit command!
                    else
                    {
                        //TODO if import -> manage file transfer (haha)

                        //TODO if export -> modify command to export to a temp local dir

                        local.Command(comm, user);
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
            if (sender.Connection != null && sender.Connection.Connected)
            {
                //TODO if export -> manage file transfer (haha)


                serverGUI.InvokeLog(sender.Name + " <- " + info);

                Byte[] data = new Byte[1 + 4 + 4 + command.Length + info.Length];

                data[0] = 3;

                BitConverter.GetBytes(command.Length).CopyTo(data, 1);
                BitConverter.GetBytes(info.Length).CopyTo(data, 5);

                Encoding.UTF8.GetBytes(command, 0, command.Length, data, 9);
                Encoding.UTF8.GetBytes(info, 0, info.Length, data, 9 + command.Length);

                SendData(sender.Connection, data);
            }
        }

        public override void ErrorMessage(string command, string message, OnlineUser sender)
        {
            if (sender.Connection != null && sender.Connection.Connected)
            {
                serverGUI.InvokeLog(sender.Name + " <- " + message);

                Byte[] data = new Byte[1 + 4 + 4 + command.Length + message.Length];

                data[0] = 4;

                BitConverter.GetBytes(command.Length).CopyTo(data, 1);
                BitConverter.GetBytes(message.Length).CopyTo(data, 5);

                Encoding.UTF8.GetBytes(command, 0, command.Length, data, 9);
                Encoding.UTF8.GetBytes(message, 0, message.Length, data, 9 + command.Length);

                SendData(sender.Connection, data);
            }
        }

        public override int Query(string message, string[] options, OnlineUser sender)
        {
            if (sender.Connection != null && sender.Connection.Connected)
            {
                serverGUI.InvokeLog(sender.Name + " <- Query: " + message);

                Byte[] data = new Byte[1 + 4 + message.Length];

                data[0] = 5;

                BitConverter.GetBytes(message.Length).CopyTo(data, 1);

                Encoding.UTF8.GetBytes(message, 0, message.Length, data, 5);

                SendData(sender.Connection, data);

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
