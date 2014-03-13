using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class VFSManager {
        private static List<VfsDisk> _disks = new List<VfsDisk>();
        private static VfsDirectory workingDirectory;
        private static VfsDisk currentDisk;

        public static void addAndOpenDisk(VfsDisk disk)
        {
            currentDisk = disk;
            _disks.Add(disk);
        }

        public static List<VfsFile> ls(bool files, bool dirs)
        {
            if (dirs && !files)
            {
                return workingDirectory.GetSubDirectories();
            }
            if (!dirs && files)
            {
                return workingDirectory.GetFiles();
            }
            return workingDirectory.Elements;
        }

        public static void cdIdent(string name)
        {
            workingDirectory = workingDirectory.GetSubDirectory(name);
            Console.WriteLine("new path: " + name);
        }

        public static void cdPath(string path)
        {
            try
            {
                var vfsDirectory = new VfsDirectory(currentDisk);
                vfsDirectory.Open(path);
                workingDirectory = vfsDirectory;
            }
            catch (InvalidCastException exception)
            {
                throw new InvalidArgumentException("cd requires a path to a folder not a file");
            }
            Console.WriteLine("new path: " + path);
        }

        public static void cp(string src, string dst, bool isRecursive)
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
