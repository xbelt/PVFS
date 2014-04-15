using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using VFS.VFS;

namespace VFS_GUI
{
    public class LocalConsole : VfsConsole
    {
        private VfsExplorer explorer;
        private ConcurrentQueue<VfsTask> tasks;
        private Thread workerThread;

        public LocalConsole(VfsExplorer explorer)
        {
            this.explorer = explorer;

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
                    explorer.setContent("", dirs, files);
                else
                {
                    explorer.setContent("", new List<string>() { "Loading..." }, new List<string>());
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
            // show a popup error message
        }

        public override void Message(string info)
        {
            // check if this is a result of ls
            // Yes: explorer.Invoke(() => explorer.setContent(path, dirs, files));
            // No: explorer.Invoke(() => statusBar.Text = info);
        }

        public override void Message(string info, ConsoleColor textCol)
        {
            Message(info);
        }

        public override int Query(string message, params string[] options)
        {
            // show a popup query (maybe windows MessageBox?)
            return 0;
        }

        public override string Readline(string message)
        {
            // is this still needed?
            return "";
        }
    }
}
