using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VFS.VFS;
using VFS_GUI;

namespace VFS_Network
{
    public partial class VfsClient : Form
    {
        private bool ready;
        VfsExplorer explorer;

        LocalConsoleAdapter localAdapter;
        RemoteConsole remote;

        public string Username;

        public VfsClient()
        {
            InitializeComponent();

            this.ready = true;

            remote = new RemoteConsole();
            localAdapter = new LocalConsoleAdapter(remote, this);
        }

        private void delayedReset()
        {
            resetTimer.Start();
        }

        public void Disconnect()
        {
            this.loginButton.Enabled = true;
            this.logoutButton.Enabled = false;
            this.explorerButton.Enabled = false;

            if (this.explorer != null)
            {
                this.explorer.Close();
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string ip = this.ipTextBox.Text;
            if (ip == "")
            {
                this.ipLabel.ForeColor = Color.Red;
                delayedReset();
                return;
            }

            int port = (int)this.portNumericUpDown.Value;
            string user = this.userTextBox.Text;
            Username = user;
            if (user.Length == 0 || user.Length >= 128)
            {
                this.userLabel.ForeColor = Color.Red;
                delayedReset();
                return;
            }

            string pass = this.passwordTextBox.Text;
            if (pass == "")
            {
                this.passwordLabel.ForeColor = Color.Red;
                delayedReset();
                return;
            }

            int res = this.localAdapter.Start(ip, port, user, pass);

            if (res == 1)
            {
                this.ipLabel.ForeColor = Color.Red;
                this.portLabel.ForeColor = Color.Red;
                delayedReset();
            }
            else if (res == 2)
            {
                this.userLabel.ForeColor = Color.Red;
                this.passwordLabel.ForeColor = Color.Red;
                delayedReset();
            }
            else
            {
                // It worked!
                this.loginButton.Enabled = false;
                this.logoutButton.Enabled = true;
                this.explorerButton.Enabled = true;
            }
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            this.localAdapter.Stop();
            this.logoutButton.ForeColor = Color.Black;
        }

        private void explorerButton_Click(object sender, EventArgs e)
        {
            this.explorer = new VfsExplorer(remote);
            this.explorer.NonLocal = true;

            this.explorer.ShowDialog(this);

            this.explorer = null;

            remote.setExplorer(null);
        }

        private void VfsClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.loginButton.Enabled)
            {
                this.ready = false;
            }
            else
            {
                e.Cancel = true;
                this.logoutButton.ForeColor = Color.Red;
            }
        }

        private void resetTimer_Tick(object sender, EventArgs e)
        {
            this.resetTimer.Stop();
            this.ipLabel.ForeColor = Color.Black;
            this.portLabel.ForeColor = Color.Black;
            this.userLabel.ForeColor = Color.Black;
            this.passwordLabel.ForeColor = Color.Black;
        }

        private void offline_Click(object sender, EventArgs e)
        {
            RemoteConsole remc = new RemoteConsole();
            VfsManager.Console = new LocalConsole(remc).StartWorker();

            this.explorer = new VfsExplorer(remc);
            this.explorer.NonLocal = false;

            this.explorer.ShowDialog(this);

            this.explorer = null;

            remote.setExplorer(null);
        }
    }
}
