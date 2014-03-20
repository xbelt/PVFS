using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class VFSManager
    {
        private static List<VfsDisk> _disks = new List<VfsDisk>();
        public static VfsDirectory workingDirectory;
        public static VfsDisk CurrentDisk;

        /// <summary>
        /// Returns the disk with the corresponding name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns a VfsDisk if it was found, otherwise null.</returns>
        private static VfsDisk getDisk(string name)
        {
            return _disks.FirstOrDefault(d => d.DiskProperties.Name == name);
        }
        /// <summary>
        /// Returns the corresponding VfsEntry.
        /// </summary>
        /// <param name="path">An absolute path containing the disk name</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        private static VfsEntry getEntry(string path)
        {
            var disk = getDisk(path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)[0]);
            if (disk != null)
                return getEntry(disk, path);
            else
                return null;
        }

        /// <summary>
        /// Returns the corresponding VfsEntry.
        /// </summary>
        /// <param name="disk">The disk to start on.</param>
        /// <param name="path">An absolute path, not containing the disk name</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        private static VfsEntry getEntry(VfsDisk disk, string path)
        {
            throw new NotImplementedException();
        }


        #region Public Interface Methods


        public static void AddAndOpenDisk(VfsDisk disk)
        {
            CurrentDisk = disk;
            _disks.Add(disk);
            workingDirectory = (VfsDirectory)EntryFactory.OpenEntry(disk, disk.root.Address, null);
            workingDirectory.Load();
        }

        public static IEnumerable<VfsEntry> ListEntries(bool files, bool dirs)
        {
            if (dirs && !files)
            {
                return workingDirectory.GetDirectories();
            }
            if (!dirs && files)
            {
                return workingDirectory.GetFiles();
            }
            return workingDirectory.GetEntries();
        }

        public static void ChangeDirectoryByIdentifier(string name)
        {
            // TODO: exception if null
            workingDirectory = workingDirectory.GetDirectory(name);
            Console.WriteLine("new path: " + name);
        }

        public static void cdPath(string path)
        {
            try
            {
                /*var vfsDirectory = new VfsDirectory(currentDisk);
                vfsDirectory.Open(path);
                workingDirectory = vfsDirectory;*/
            }
            catch (InvalidCastException exception)
            {
                throw new InvalidArgumentException("cd requires a path to a folder not a file");
            }
            Console.WriteLine("new path: " + path);
        }

        /// <summary>
        /// Moves a File or Directory to a new location. A directory is moved recursively.
        /// </summary>
        /// <param name="srcPath">The absolute path to the File/Directory that should be moved.</param>
        /// <param name="dstPath">The absolute path to the target Directory.</param>
        public static void move(string srcPath, string dstPath)
        {
            if (srcPath == null || dstPath == null)
                throw new ArgumentException("Argument null.");
            if (dstPath.StartsWith(srcPath)) // TODO: make a real recursive test
                throw new ArgumentException("Can't move a directory into itself!");

            VfsFile src = (VfsFile) getEntry(srcPath);
            VfsEntry dst = getEntry(dstPath);

            if (src == null || dst == null)
                throw new ArgumentException("Src or Dst did not exist.");
            if (!dst.IsDirectory)
                throw new ArgumentException("Destination must be a directory.");

            VfsDirectory parent = src.Parent;
            parent.RemoveElement(src);
            VfsDirectory target = (VfsDirectory) dst;
            target.AddElement(src);

            Console.WriteLine("copy " + src + " to " + dst);
        }

        public static void cp(string src, string dst, bool isRecursive)
        {
            Console.WriteLine("copy " + src + " to " + dst);
        }

        public static void navigateUp()
        {
            throw new NotImplementedException();
        }

        public static void UnloadDisk(string name)
        {
            var unmountedDisks = _disks.Where(x => x.DiskProperties.Name == name);
            foreach (var unmountedDisk in unmountedDisks)
            {
                unmountedDisk.getReader().Close();
                unmountedDisk.getWriter().Close();
            }
        }

        #endregion

        public static void Import(string src, string dst)
        {
            throw new NotImplementedException();
        }

        public static void Export(string dst, string src)
        {
            throw new NotImplementedException();
        }

        public static void Remove(string text, bool isDirectory)
        {
        }
    }
}
