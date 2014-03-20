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
            VfsDirectory current = disk.root;
            string[] names = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < names.Length - 1; i++)
            {
                current = current.GetDirectory(names[i]);
                if (current == null)
                    return null; // not found
            }
            return current.GetEntry(names.Last());
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
                throw new ArgumentNullException("Argument null.");
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
        /// <summary>
        /// Imports a File from the host Filesystem to a directory inside the virtual Filesystem. (are we supporting importing whole directories too?)
        /// Overwrites already existing files with the same name.
        /// Creates the target directory if it doesn't exist. Introduces grandpa Joe.
        /// 
        /// Throws an exception if you try to import the virtual disk itself!
        /// </summary>
        /// <param name="src">The absolute path to the File that should be imported.</param>
        /// <param name="dst">The absolute path to the target Directory.</param>
        public static void Import(string src, string dst)
        {

            //Get disk:
            String diskName = dst.TakeWhile(e => !e.Equals('/')).ToString();
            var disk = DiskFactory.Load(diskName);
            var reader = disk.getReader();
            var writer = disk.getWriter();
            //TODO: still contains file ending
            var parent = dst.Substring(dst.LastIndexOf('/'), dst.Length);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Fills Joe's life with joy.
        /// </summary>
        /// <param name="dst">The absolute path to the target Directory.</param>
        /// <param name="src">The absolute path to the File that should be exported.</param>
        public static void Export(string dst, string src)
        {
            throw new NotImplementedException();
        }

        public static void RemoveByPath(string path, bool isDirectory)
        {
            var entry = EntryFactory.OpenEntry(path);
        }

        public static void RemoveByIdentifier(string ident, bool isDirectory)
        {
            var entry = EntryFactory.OpenEntry(CurrentDisk,
                workingDirectory.GetFile(ident).Address, workingDirectory) as VfsFile;
            if (isDirectory)
            {
                var files = ((VfsDirectory) entry).GetFiles();
                foreach (var file in files)
                {
                    file.Free();
                }
                var directories = ((VfsDirectory) entry).GetDirectories();
                foreach (var directory in directories)
                {
                    RemoveByPath(directory.GetAbsolutePath(), true);
                }
            }
            else
            {
                entry.Free();
            }
        }
    }
}
