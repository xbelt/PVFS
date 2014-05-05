using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VFS_GUI;
using VFS.VFS;
using System.Net;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace VFS_Network
{
    public partial class VfsServer : Form
    {
        public bool ready;

        RemoteConsoleAdapter remoteAdapter;
        LocalConsole local;

        public List<VfsUser> Users { get; private set; }


        public VfsServer()
        {
            InitializeComponent();

            this.ready = true;

            this.remoteAdapter = new RemoteConsoleAdapter(this);
            VfsManager.Console = this.local = new LocalConsole(remoteAdapter).StartWorker();

            this.Users = new List<VfsUser>();
        }

        private void VfsServer_Load(object sender, EventArgs e)
        {
            // Local IP

            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress add in ipEntry.AddressList)
            {
                if (add.ToString().StartsWith("192.168."))
                    this.localIPLabel.Text = add.ToString();
            }

            // Public IP async

            new Thread(this.VfsServer_LoadPublicIP).Start();

            // Users

            string path = Environment.CurrentDirectory + "\\users.bin";
            if (File.Exists(path))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                this.Users = (List<VfsUser>) formatter.Deserialize(stream);
                stream.Close();
            }

            this.userListView.Items.Clear();
            foreach (VfsUser user in this.Users)
            {
                user.Online = false;
                addUser(user);
            }
        }

        private void VfsServer_LoadPublicIP()
        {
            string url = "http://checkip.dyndns.org";
            WebRequest req = WebRequest.Create(url);
            StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream());
            string response = sr.ReadToEnd().Trim();

            int i = response.IndexOf(':');
            int j = response.IndexOf('<', i + 1);

            if (j - i - 2 > 0)
            {
                string ip = response.Substring(i + 2, j - i - 2);

                this.Invoke(new Action(() => this.publicIPLabel.Text = ip));
            }
        }

        //-----------------Interface-----------------

        public void Log(string info)
        {
            if (this.logTextBox.Text.Length > 4096)
            {
                this.logTextBox.Text = this.logTextBox.Text.Substring(this.logTextBox.Text.Length - 4095);
            }

            this.logTextBox.Text += info + "\r\n";
            this.logTextBox.Select(this.logTextBox.Text.Length, 0);
            this.logTextBox.ScrollToCaret();
        }

        public void InvokeLog(string info)
        {
            if (this.ready)
                this.Invoke(new Action(() => this.Log(info)));
        }

        public void EnableStartServer()
        {
            this.startServerButton.Enabled = true;
        }

        public void SetOnlineState(VfsUser user)
        {
            foreach (ListViewItem lvi in this.userListView.Items)
            {
                if (lvi.Tag == user)
                {
                    lvi.ImageIndex = user.Online ? 1 : 0;
                    return;
                }
            }
        }


        public static Byte[] GetHash(string password)
        {
            Byte[] plain = Encoding.UTF8.GetBytes(password);

            HashAlgorithm algorithm = new SHA256Managed();

            return algorithm.ComputeHash(plain);
        }

        //-----------------Private-----------------

        private void addUser(VfsUser user)
        {
            ListViewItem lvi = this.userListView.Items.Add(user.Name);
            lvi.Tag = user;
            lvi.ImageIndex = 0;
        }

        //-----------------Buttons & Co.-----------------

        public void startServerButton_Click(object sender, EventArgs e)
        {
            this.startServerButton.ForeColor = Color.Black;
            if (this.portNumericUpDown.Enabled)
            {
                this.startServerButton.Text = "Stop";
                this.portNumericUpDown.Enabled = false;
                this.remoteAdapter.Start((int)this.portNumericUpDown.Value);
            }
            else
            {
                this.startServerButton.Text = "Start";
                this.portNumericUpDown.Enabled = true;
                this.startServerButton.Enabled = false;
                this.remoteAdapter.Stop();
            }
        }

        private void addUserButton_Click(object sender, EventArgs e)
        {
            AddUserDialog aud = new AddUserDialog();
            if (aud.ShowDialog() == DialogResult.OK)
            {
                if (this.Users.Any(u => u.Name == aud.ResultName))
                {
                    MessageBox.Show("This user already exists.", "Virtual File System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                VfsUser user = new VfsUser() { Name = aud.ResultName, PasswordHash = GetHash(aud.ResultPassword) };

                this.Users.Add(user);
                addUser(user);
            }
        }

        private void deleteUserButton_Click(object sender, EventArgs e)
        {
            if (this.userListView.SelectedIndices.Count == 1)
            {
                ListViewItem lvi = this.userListView.SelectedItems[0];
                VfsUser user = (VfsUser)lvi.Tag;
                if (!user.Online)
                {
                    this.Users.Remove(user);
                    lvi.Remove();
                }
            }
        }

        private void VfsServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.portNumericUpDown.Enabled)
            {
                this.ready = false;
                local.Command("quit", OnlineUser.Empty);
                
                string path = Environment.CurrentDirectory + "\\users.bin";
                if (File.Exists(path))
                    File.Delete(path);
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, this.Users);
                stream.Close();
            }
            else
            {
                e.Cancel = true;
                this.startServerButton.ForeColor = Color.Red;
            }
        }
    }
}
