using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VFS.VFS;

namespace VFS_GUI
{
    /// <summary>
    /// This is the console which speaks to the VfsExplorer.
    /// </summary>
    public class RemoteConsole
    {
        protected const string caption = "Virtual File System";

        private VfsExplorer explorer;
        protected LocalConsole local;

        public RemoteConsole()
        {

        }

        public virtual void setExplorer(VfsExplorer explorer)
        {
            this.explorer = explorer;
        }

        public void setConsole(LocalConsole local)
        {
            this.local = local;
        }

        /// <summary>
        /// Sends the command to the LocalConsole.
        /// </summary>
        /// <param name="comm"></param>
        public virtual void Command(string comm)
        {
            local.Command(comm, null);
        }

        /// <summary>
        /// Is called when a result from the localConsole is received.
        /// </summary>
        /// <param name="info"></param>
        public virtual void Message(string command, string info, OnlineUser sender)
        {
            if (explorer != null && explorer.Ready)
            {
                if (command.StartsWith("ls"))
                {
                    string[] dirs, files;
                    var lines = info.Split('\n');
                    dirs = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    files = lines[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    explorer.Invoke(new Action(() => explorer.ReceiveListEntries(command.Substring(3), dirs, files)));
                }
                else if (command.StartsWith("search"))
                {
                    string[] dirs, files;
                    var lines = info.Split('\n');
                    files = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    dirs = lines[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    explorer.Invoke(new Action(() => explorer.ReceiveSearchResults(files, dirs)));
                }
                else if (command.StartsWith("ldisks"))
                {
                    string[] disks;
                    disks = info.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    explorer.Invoke(new Action(() => explorer.ReceiveListDisks(disks)));
                }
                else
                {
                    explorer.Invoke(new Action(() => explorer.SetStatus(command, info)));
                }
            }
        }

        /// <summary>
        /// Is called when a result from the localConsole is received.
        /// </summary>
        /// <param name="message"></param>
        public virtual void ErrorMessage(string command, string message, OnlineUser sender)
        {
            if (explorer != null && explorer.Ready)
                explorer.Invoke(new Action(() => explorer.ReceivedError(command, message)));
        }

        /// <summary>
        /// Is called when the LocalConsole needs a user query.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual int Query(string message, string[] options, OnlineUser sender)
        {
            DialogResult res = MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.YesNo);
            return res == DialogResult.Yes ? 0 : 1;
        }

        public virtual void SetReady()
        {
            if (explorer != null && explorer.Ready)
                explorer.Invoke(new Action(explorer.SetReady));
        }

        public virtual void SetBusy()
        {
            if (explorer != null && explorer.Ready)
                explorer.Invoke(new Action(explorer.SetBusy));
        }
    }
}
