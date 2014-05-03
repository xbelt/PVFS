using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VFS.VFS;

namespace VFS_GUI
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            RemoteConsole<object> remc = new RemoteConsole<object>();
            VfsManager.Console = new LocalConsole<object>(remc);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VfsExplorer(remc));
        }
    }
}
