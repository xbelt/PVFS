using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VFS_Network
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0 && args[0].ToLower() == "server")
            {
                Application.Run(new VfsServer());
            }
            else if (args.Length > 0 && args[0].ToLower() == "client")
            {
                Application.Run(new VfsClient());
            }
            else
            {
                ModeChoiceDialog mcd = new ModeChoiceDialog();
                Application.Run(mcd);
                if (mcd.DialogResult == DialogResult.OK)
                {
                    if (mcd.Server)
                    {
                        Application.Run(new VfsServer());
                    }
                    else
                    {
                        Application.Run(new VfsClient());
                    }
                }
            }
        }
    }
}
