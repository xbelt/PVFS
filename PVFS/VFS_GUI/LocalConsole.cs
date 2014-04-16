using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using VFS.VFS;

namespace VFS_GUI
{
    /// <summary>
    /// This is the console which speaks to the VfsManager.
    /// </summary>
    public class LocalConsole : VfsConsole
    {
        private RemoteConsole remote;

        private ConcurrentQueue<VfsTask> tasks;
        private Thread workerThread;
        private string lastCommand;

        public LocalConsole(RemoteConsole remote)
        {
            this.remote = remote;
            remote.setConsole(this);

            this.tasks = new ConcurrentQueue<VfsTask>();

            this.workerThread = new Thread(new ThreadStart(this.workerThreadProcedure));
            this.workerThread.Name = "VFS Worker Thread";
            this.workerThread.Start();
        }

        private void workerThreadProcedure()
        {
            VfsTask task;
            while (true)
            {
                if (tasks.TryDequeue(out task))
                {
                    if (task.Command == "quit")
                        return;

                    VfsManager.ExecuteCommand(task.Command);
                }
            }
        }


        public override void Command(string comm)
        {
            if (comm.StartsWith("ls"))
            {
                List<string> dirs, files;
                int res = VfsManager.ListEntriesConcurrent(comm.Substring(3), out dirs, out files);
                if (res == 1)
                {
                    this.ErrorMessage("Invalid path.");
                    return;
                }
                if (res == 0)
                    remote.setContent(dirs, files);
                else
                {
                    remote.setContent(new List<string>() { "Loading..." }, new List<string>());
                    tasks.Enqueue(new VfsTask() { Command = comm });
                }
            }
            else if (comm == "free" || comm == "occ")
            {
                VfsManager.ExecuteCommand(comm);
            }
            else
            {
                tasks.Enqueue(new VfsTask() { Command = comm });
            }
        }

        public override void ErrorMessage(string message)
        {
            remote.ErrorMessage(message);
        }

        public override void Message(string info)
        {
            // check if this is a result of ls
            // Yes: explorer.Invoke(() => explorer.setContent(path, dirs, files));
            // No: explorer.Invoke(() => statusBar.Text = info);
            remote.Message(info);
        }

        public override void Message(string info, ConsoleColor textCol)
        {
            Message(info);
        }

        public override int Query(string message, params string[] options)
        {
            // show a popup query (maybe windows MessageBox?)
            return remote.Query(message, options);
        }
    }
}
