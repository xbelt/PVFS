﻿using System;
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

        private ConcurrentQueue<VfsTask<object>> tasks;
        private VfsTask<object> currentTask;
        private Thread workerThread;

        public LocalConsole(RemoteConsole remote)
        {
            this.remote = remote;
            remote.setConsole(this);

            this.tasks = new ConcurrentQueue<VfsTask<object>>();
            this.currentTask = new VfsTask<object>() { Command = "", Sender = null };

            this.workerThread = new Thread(new ThreadStart(this.workerThreadProcedure));
            this.workerThread.Name = "VFS Worker Thread";
            this.workerThread.Start();
        }

        private void workerThreadProcedure()
        {
            VfsTask<object> task;
            while (true)
            {
                if (tasks.TryDequeue(out task))
                {
                    remote.SetBusy();

                    currentTask = task;
                    VfsManager.ExecuteCommand(task.Command);

                    remote.SetReady();

                    if (task.Command == "quit")
                        return;
                }
            }
        }


        public override void Command(string comm, object sender)
        {
            if (comm.StartsWith("ls"))
            {
                List<string> dirs, files;
                int res = VfsManager.ListEntriesConcurrent(comm.Substring(3), out dirs, out files);
                if (res == 1)
                {
                    remote.ErrorMessage(comm, "Invalid path.", sender);
                    return;
                }
                if (res == 0)
                {
                    remote.Message(comm, dirs.Concat(" ") + "\n" + files.Concat(" "), sender);
                }
                else
                {
                    tasks.Enqueue(new VfsTask<object>() { Command = comm, Sender = sender });
                }
            }
            else if (comm == "free" || comm == "occ")
            {
                VfsManager.ExecuteCommand(comm);
            }
            else
            {
                tasks.Enqueue(new VfsTask<object>() { Command = comm, Sender = sender });
            }
        }

        public override void ErrorMessage(string message)
        {
            remote.ErrorMessage(currentTask.Command, message, currentTask.Sender);
        }

        public override void Message(string info)
        {
            remote.Message(currentTask.Command, info, currentTask.Sender);
        }

        public override void Message(string info, ConsoleColor textCol)
        {
            Message(info);
        }

        public override int Query(string message, params string[] options)
        {
            return remote.Query(message, options, currentTask.Sender);
        }
    }
}
