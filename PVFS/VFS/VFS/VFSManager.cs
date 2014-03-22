using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security;
using VFS.VFS.Models;
using VFS.VFS.Parser;

namespace VFS.VFS
{
    class VFSManager
    {
        private static List<VfsDisk> _disks = new List<VfsDisk>();
        public static VfsDirectory workingDirectory;
        public static VfsDisk CurrentDisk;
        public static VfsConsole Console = new VfsConsole();

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
        /// Returns the corresponding VfsEntry. Does not return the root directory.
        /// </summary>
        /// <param name="path">An absolute path containing the disk name</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        private static VfsEntry getEntry(string path)
        {
            VfsDirectory last;
            return getEntry(path, out last);
        }

        /// <summary>
        /// Returns the corresponding VfsEntry. Does not return the root directory.
        /// </summary>
        /// <param name="path">An absolute path containing the disk name</param>
        /// <param name="last">Returns the last found directory (usefull if not the whole path exists). Can be root. Null if disk was not found</param>
        /// <param name="remaining">Returns the remaining path, if the result was null.</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        private static VfsEntry getEntry(string path, out VfsDirectory last, out IEnumerable<string> remaining)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            int i = path.IndexOf('/', 1);
            if (i == -1)
                throw new ArgumentException("Path not valid.");
            var disk = getDisk(path.Substring(1, i - 1));
            if (disk != null)
                return getEntry(disk, path.Substring(i + 1), out last, out remaining);
            else
            {
                last = null;
                remaining = path.Split(new[] {'/'}, StringSplitOptions.None);
                return null;
            }
        }

        /// <summary>
        /// Returns the corresponding VfsEntry. Does not return the root directory.
        /// </summary>
        /// <param name="disk">The disk to start on.</param>
        /// <param name="path">An absolute path, not containing the disk name</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        private static VfsEntry getEntry(VfsDisk disk, string path)
        {
            VfsDirectory last;
            return getEntry(disk, path, out last);
        }

        /// <summary>
        /// Returns the corresponding VfsEntry. Does not return the root directory.
        /// </summary>
        /// <param name="disk">The disk to start on.</param>
        /// <param name="path">An absolute path, not containing the disk name</param>
        /// <param name="last">Returns the last found directory (usefull if not the whole path exists). Can be root.</param>
        /// <param name="remaining">Returns the remaining path, if the result was null.</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        private static VfsEntry getEntry(VfsDisk disk, string path, out VfsDirectory last, out IEnumerable<string> remaining)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            VfsDirectory current = disk.root;
            string[] names = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 0)
                throw new ArgumentException("Path not valid. Root can't be accessed this way.");
            for (int i = 0; i < names.Length - 1; i++)
            {
                last = current;
                current = current.GetDirectory(names[i]);
                if (current == null)
                {
                    remaining = names.Skip(i);
                    return null; // not found
                }
            }
            last = current;
            remaining = names.Skip(names.Length - 1);
            return current.GetEntry(names.Last());
        }

        /// <summary>
        /// Returns a path to a non-existent file in the host file system.
        /// </summary>
        /// <returns>The path.</returns>
        private static string getTempFilePath()
        {
            string path;
            int i = 0;
            do
            {
                path = Environment.CurrentDirectory + "\\temp" + i++;
            } while (File.Exists(path));
            return path;
        }


        #region Public Interface Methods


        public static void AddAndOpenDisk(VfsDisk disk)
        {
            CurrentDisk = disk;
            _disks.Add(disk);
            workingDirectory = (VfsDirectory)EntryFactory.OpenEntry(disk, disk.root.Address, null);
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
            workingDirectory = workingDirectory.GetDirectory(name) ?? workingDirectory;
            Console.WriteLine("new path: " + name);
        }

        public static void cdPath(string path)
        {
            try
            {
                /*var vfsDirectory = new VfsDirectory(currentDisk);
                vfsDirectory.Open(path);
                workingDirectory = vfsDirectory;*/

                /* try this
                VfsEntry e = getEntry(path);
                if (e == null)
                    throw new ArgumentException("Invalid path.");
                if (e.IsDirectory)
                    workingDirectory = (VfsDirectory) e;
                else
                    throw new ArgumentException("cd requires a path to a folder not a file");
                */
            }
            catch (InvalidCastException exception)
            {
                throw new ArgumentException("cd requires a path to a folder not a file");
            }
            Console.WriteLine("new path: " + path);
        }

        public static void CreateFile(string path)
        {
            
        }

        /// <summary>
        /// Creates directories such that the whole path given exists. If the path contains a file this returns false/an exception?
        /// </summary>
        /// <param name="path">The path to create.</param>
        public static void createDirectory(string path)
        {
            VfsDirectory last;
            IEnumerable<string> remainingPath;

            VfsEntry entry = getEntry(path, out last, out remainingPath);

            List<string> remaining = remainingPath.ToList();

            if (entry == null)
            {
                if (last == null)
                {
                    string diskName = remaining.Any() ? remaining.First() : "Unknown";
                    Console.Message("The Disk " + diskName + " does not exist!");
                    return;
                }

                foreach (string name in remaining)
                {
                    VfsEntry next = last.GetEntry(name);
                    if (next == null)
                    {
                        // create
                        if (name.Length > VfsFile.MaxNameLength)
                        {
                            Console.Message("The name of the directory was too long.");
                            return;
                        }
                        VfsDirectory newDir = EntryFactory.createDirectory(last.Disk, name, last);
                        last.AddElement(newDir);
                        last = newDir;
                    }
                    else if (next.IsDirectory)
                    {
                        // go on
                        last = (VfsDirectory)next;
                    }
                    else
                    {
                        // invalid path
                        Console.Message("This path leads to a file.");
                        return;
                    }
                }
            }
            else
            {
                Console.Message(entry.IsDirectory ? "The Directory already existed." : "This path leads to a file.");
                return;
            }
        }


        /// <summary>
        /// Moves a File or Directory to a new location. A directory is moved recursively.
        /// </summary>
        /// <param name="srcPath">The absolute path to the File/Directory that should be moved.</param>
        /// <param name="dstPath">The absolute path to the target Directory.</param>
        public static void move(string srcPath, string dstPath)
        {
            if (srcPath == null || dstPath == null)
                throw new ArgumentNullException("", "Argument null.");
            if (dstPath.StartsWith(srcPath)) // TODO: make a real recursive test
                throw new ArgumentException("Can't move a directory into itself!");

            VfsEntry srcEntry = getEntry(srcPath);
            VfsEntry dstEntry = getEntry(dstPath);

            if (srcEntry == null || dstEntry == null)
                throw new ArgumentException("Src or Dst did not exist.");
            if (!dstEntry.IsDirectory)
                throw new ArgumentException("Destination must be a directory.");

            VfsFile src = (VfsFile)srcEntry;
            VfsDirectory dst = (VfsDirectory)dstEntry;

            if (src.Disk != dst.Disk)
            {
                if (src.IsDirectory)
                    throw new ArgumentException("Can't move a directory to another disk.");

                string tmp = getTempFilePath();

                Export(tmp, srcPath);
                Import(tmp, dstPath);
                RemoveByPath(srcPath, false);
                File.Delete(tmp);
            }
            else
            {
                src.Parent.RemoveElement(src);
                dst.AddElement(src);
            }
            Console.WriteLine("copy " + src + " to " + dstEntry);
        }

        /// <summary>
        /// Copies a file/directory into a target directory. Directories will be copied recursively.
        /// If the target directory does not exist it will be created.
        /// </summary>
        /// <param name="srcPath">The path to the file/directory which should be copied.</param>
        /// <param name="dstPath">The target directory.</param>
        public static void Copy(string srcPath, string dstPath)
        {
            if (srcPath == null || dstPath == null)
                throw new ArgumentNullException("", "Argument null.");

            VfsEntry srcEntry = getEntry(srcPath);
            VfsDirectory last;
            IEnumerable<string> remainingPath;
            VfsEntry dstEntry = getEntry(dstPath, out last, out remainingPath);

            if (srcEntry == null)
                throw new ArgumentException("Source did not exist.");
            if (dstEntry != null && !dstEntry.IsDirectory)
                throw new ArgumentException("Destination must be a directory.");

            if (dstEntry == null)
            {
                if (last == null)
                {
                    string diskName = remainingPath.Any() ? remainingPath.First() : "Unknown";
                    Console.Message("The Disk " + diskName + " does not exist!");
                    return;
                }
                // Create dst using last.
            }

            if (srcEntry.IsDirectory)
            {
                // copy directory.
                // if directory already exists: cancel
            }
            else
            {
                VfsFile src = (VfsFile)srcEntry;
                // copy file
            }






            Console.WriteLine("copy " + srcPath + " to " + dstPath);
        }

        private static void copyHelper(VfsEntry src, VfsDirectory dst)
        {

        }

        public static void navigateUp()
        {
            throw new NotImplementedException();
            /*
            if (workingDirectory.Parent == null) // we're root
                throw new InvalidStateException("or do nothing, which might be smarter.");
            else
                workingDirectory = workingDirectory.Parent;
            */
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
        /// <param name="src">The absolute path to the File that should be imported (Host file system).</param>
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
        /// <param name="dst">The absolute path to the target Directory (Host file system).</param>
        /// <param name="src">The absolute path to the File that should be exported.</param>
        public static void Export(string dst, string src)
        {
            throw new NotImplementedException();
        }

        public static void RemoveByPath(string path, bool isDirectory)
        {
            var entry = EntryFactory.OpenEntry(path) as VfsFile;
            if (isDirectory)
            {
                var files = ((VfsDirectory)entry).GetFiles();
                foreach (var file in files)
                {
                    file.Free();
                }
                var directories = ((VfsDirectory)entry).GetDirectories();
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

        public static void RemoveByIdentifier(string ident, bool isDirectory)
        {
            var entry = EntryFactory.OpenEntry(CurrentDisk,
                workingDirectory.GetFile(ident).Address, workingDirectory) as VfsFile;
            // TODO: this creates a new object of a possibliy already existing file/dir! We have errors if there are multiple objects of the same file/dir.
            /* Use this:
            var entry = getEntry(CurrentDisk, workingDirectory.GetAbsolutePath() + "/" + ident);
            */
            if (isDirectory)
            {
                var files = ((VfsDirectory)entry).GetFiles();
                foreach (var file in files)
                {
                    file.Free();
                }
                var directories = ((VfsDirectory)entry).GetDirectories();
                foreach (var directory in directories)
                {
                    RemoveByPath(directory.GetAbsolutePath(), true);
                }
                // TODO free entry
            }
            else
            {
                entry.Free();
            }
        }

        public static VfsDisk GetDisk(string diskName)
        {
            if (_disks.Any(x => x.DiskProperties.Name == diskName))
            {
                _disks.Single(x => x.DiskProperties.Name == diskName);
            }
            throw new DiskNotFoundException();
        }
    }
}
