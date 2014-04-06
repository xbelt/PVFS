using System;
using VFS.VFS;

namespace VFS
{
    class Program
    {
        static void Main()
        {
            while (true)
            {
                var value = Console.ReadLine();
                VfsManager.ExecuteCommand(value);
            }
        }
    }
}
