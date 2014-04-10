using System;
using System.Collections.Generic;
using System.Threading;
using VFS.VFS;

namespace VFS_GUI
{
    public class LocalConsole : VfsConsole
    {
        private Thread workerThread;
        private Queue<VfsTask> tasks;
        private MainWindow window;

        public LocalConsole(MainWindow window)
        {
            this.window = window;
        }

        public override void Command(string comm)
        {
            // check if ls, help, free, occ, etc. (we never call help from this interface)
            // Yes: carry out directly (call readonly version of ls, if not available -> add to queue)
            // No: add to queue
        }

        public override void ErrorMessage(string message)
        {
            // show a popup error message
        }

        public override void Message(string info)
        {
            // check if this is a result of ls
            // Yes: maybe display (check if it's the correct result of ls)
            // No: window.Dispatcher.Invoke(() => statusBar.Text = info);
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
