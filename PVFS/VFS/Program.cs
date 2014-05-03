using System;
using VFS.VFS;

namespace VFS
{
    class Program
    {
        static void Main()
        {
            VfsConsole console = new VfsConsole();
            VfsManager.Console = console;

            while (true)
            {
                var value = Console.ReadLine();
                console.Command(value, null);
            }
        }
    }
}
