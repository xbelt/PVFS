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

        public static List<VfsEntry> ls()
        {
            Console.WriteLine("LS");
            return null;
        }

        public static void cd(string path)
        {
            Console.WriteLine("new path: " + path);
        }
    }
}
