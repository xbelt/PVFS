using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class VFSManager {
        private static List<VfsDisk> _disks;
        private static VfsDirectory workingDirectory;

        public static List<VfsEntry> ls(string path) {
            throw new NotImplementedException();
        }

        public static List<VfsFile> ls()
        {
            return workingDirectory.Elements;
        }

        public static void cd(string path)
        {
            try
            {
                workingDirectory = new VfsDirectory().Open(path) as VfsDirectory;
            }
            catch (InvalidCastException exception)
            {
                throw new InvalidArgumentException("cd requires a path to a folder not a file");
            }
            Console.WriteLine("new path: " + path);
        }

        public static void cp(string src, string dst)
        {
            Console.WriteLine("copy " + src + " to " + dst);
        }
    }

    internal class InvalidArgumentException : Exception
    {
        public InvalidArgumentException(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
