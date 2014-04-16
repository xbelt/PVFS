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
    public class RemoteConsole : VfsConsole
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
        public override void Command(string comm)
        {
            local.Command(comm);
        }

        /// <summary>
        /// Is called when a result from the localConsole is received.
        /// </summary>
        /// <param name="info"></param>
        public override void Message(string info)
        {
            explorer.Invoke(new Action(() => explorer.setStatus(info)));
        }

        /// <summary>
        /// Is called when a result from the localConsole is received.
        /// </summary>
        /// <param name="message"></param>
        public override void ErrorMessage(string message)
        {
            explorer.Invoke(new Action(() => 
                MessageBox.Show(explorer, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error)));
        }

        /// <summary>
        /// Is called when the LocalConsole needs a user query.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override int Query(string message, params string[] options)
        {
            DialogResult res = MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.YesNo);
            return res == DialogResult.Yes ? 0 : 1;
        }

        internal void setContent(List<string> dirs, List<string> files)
        {
            explorer.Invoke(new Action(() => explorer.SetContent(dirs, files)));
        }
    }
}
