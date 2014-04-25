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
        private const string caption = "Virtual File System";

        private VfsExplorer explorer;
        private LocalConsole local;

        public RemoteConsole(VfsExplorer explorer)
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
        public void Command(string comm)
        {
            local.Command(comm);
        }

        /// <summary>
        /// Is called when a result from the localConsole is received.
        /// </summary>
        /// <param name="info"></param>
        public void Message(string command, string info)
        {
            if (command.StartsWith("ls"))
            {
                string[] dirs, files;
                var lines = info.Split('\n');
                dirs = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                files = lines[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                explorer.Invoke(new Action(() => explorer.ReceiveListEntries(command.Substring(3), dirs, files)));
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

        /// <summary>
        /// Is called when a result from the localConsole is received.
        /// </summary>
        /// <param name="message"></param>
        public void ErrorMessage(string command, string message)
        {
            if (command.StartsWith("ls"))
            {
                explorer.Invoke(new Action(() => explorer.ReceivedInvalidDirectory(command.Substring(3))));
            }
            else
            {
                explorer.Invoke(new Action(() =>
                {
                    MessageBox.Show(explorer, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        /// <summary>
        /// Is called when the LocalConsole needs a user query.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public int Query(string message, params string[] options)
        {
            DialogResult res = MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.YesNo);
            return res == DialogResult.Yes ? 0 : 1;
        }
    }
}
