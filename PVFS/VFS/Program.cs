using System;
using VFS.VFS;

namespace VFS
{
    class Program
    {
        static void Main()
        {
            VfsConsole<object> console = new VfsConsole<object>();
            VfsManager.Console = console;

            while (true)
            {
                var value = Console.ReadLine();
                console.Command(value, null);
            }
        }
    }
}
