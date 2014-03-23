using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static string[] idToSize = new[] {"byte", "kb", "mb", "gb", "tb"};

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
            IEnumerable<string> remaining;
            return getEntry(path, out last, out remaining);
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
                remaining = path.Split(new[] { '/' }, StringSplitOptions.None);
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
            IEnumerable<string> remaining;
            return getEntry(disk, path, out last, out remaining);
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
            workingDirectory = workingDirectory.GetDirectory(name) ?? workingDirectory;
            Console.Message("New working directory: " + name);
        }

        public static void cdPath(string path)
        {
            try
            {
                var pathElements = path.Substring(1).Split('/');
                CurrentDisk = GetDisk(pathElements[0]);
                workingDirectory = EntryFactory.OpenEntry(path) as VfsDirectory;
            }
            catch (InvalidCastException exception)
            {
                Console.Error("cd requires a path to a folder not a file");
                return;
            }
            Console.Message("New working directory: " + path);
        }

        /// <summary>
        /// Creates an empty file at the indicated location. Returns early if the target directory doesn't exist.
        /// </summary>
        /// <param name="path">The path to the new file.</param>
        public static void CreateFile(string path)
        {
            VfsDirectory last;
            IEnumerable<string> remaining;
            VfsEntry entry = getEntry(path, out last, out remaining);

            if (entry != null)
            {
                Console.Error("There already existed a file or directory with the same path.");
                return;
            }

            if (last == null)
            {
                Console.Error("The disk was not found.");
                return;
            }

            List<string> fileNames =  remaining.ToList();
            if (fileNames.Count > 1)
            {
                Console.Error("Target directory was not found.");
                return;
            }

            VfsFile file = EntryFactory.createFile(last.Disk, fileNames[0], 0, last);
            last.AddElement(file);
        }

        /// <summary>
        /// Creates directories such that the whole path given exists. If the path contains a file this does nothing.
        /// </summary>
        /// <param name="path">The path to create.</param>
        /// <returns>Returns true if the operation succeded, otherwise false.</returns>
        public static bool createDirectory(string path)
        {
            VfsDirectory last;
            IEnumerable<string> remainingPath;

            VfsEntry entry = getEntry(path, out last, out remainingPath);

            List<string> remaining = remainingPath.ToList();

            if (entry == null)
            {
                if (last == null)
                {
                    if (remaining.Any())
                        Console.Error("The Disk " + remaining.First() + " does not exist!");
                    else
                        Console.Error("Please enter a valid path.");
                    return false;
                }

                foreach (string name in remaining)
                {
                    VfsEntry next = last.GetEntry(name);
                    if (next == null)
                    {
                        // create
                        if (name.Length > VfsFile.MaxNameLength)
                        {
                            Console.Error("The name of the directory was too long.");
                            return false;
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
                        Console.Error("A file with the same name already existed.");
                        return false;
                    }
                }
                return true;
            }
            else
            {
                if (entry.IsDirectory)
                {
                    Console.Message("This Directory already existed.");
                    return true;
                }
                else
                {
                    Console.Error("A file with the same name already existed.");
                    return false;
                }
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
            {
                Console.Error("Can't move a directory into itself!");
                return;
            }

            VfsEntry srcEntry = getEntry(srcPath);
            VfsEntry dstEntry = getEntry(dstPath);

            if (srcEntry == null || dstEntry == null)
            {
                Console.Error("The source or the destination did not exist.");
                return;
            }
            if (!dstEntry.IsDirectory)
            {
                Console.Error("The destination must be a directory.");
                return;
            }

            VfsFile src = (VfsFile)srcEntry;
            VfsDirectory dst = (VfsDirectory)dstEntry;

            if (src.Disk != dst.Disk)
            {
                if (src.IsDirectory)
                {
                    Console.Error("Can't move a directory to another disk.");
                    return;
                }

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
            Console.Message("copy " + src + " to " + dstEntry);
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
            {
                Console.Error("Source did not exist.");
                return;
            }
            if (dstEntry != null && !dstEntry.IsDirectory)
            {
                Console.Error("Destination must be a directory.");
                return;
            }

            if (dstEntry == null)
            {
                // Create dst using last.
                if (!createDirectory(dstPath))
                {
                    Console.Error("Copying was canceled.");
                    return;
                }
                dstEntry = getEntry(dstPath);
            }
            VfsDirectory dst = (VfsDirectory)dstEntry;

            if (!copyHelper(srcEntry, dst, true))
            {
                Console.Error("Copying was canceled due to a too long file or directory name.");
                return;
            }

            Console.Message("copy " + srcPath + " to " + dstPath);
        }

        /// <summary>
        /// A recursive helper function for Copy().
        /// </summary>
        /// <param name="src">The file/dir to copy.</param>
        /// <param name="dst">The target directory.</param>
        /// <param name="first">Is this the first invocation of the recursive call?</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private static bool copyHelper(VfsEntry src, VfsDirectory dst, bool first)
        {
            // find name for copy
            string newName = src.Name + (first ? "(copy)" : "");
            int i = 2;
            while (dst.GetEntry(newName) != null)
                newName = src.Name + "(copy_" + i++ + ")";

            if (newName.Length > VfsFile.MaxNameLength)
                return false;

            // check if file or dir
            if (src.IsDirectory)
            {
                // dir: create, recursive calls
                VfsDirectory newDir = EntryFactory.createDirectory(dst.Disk, newName, dst);
                dst.AddElement(newDir);
                return ((VfsDirectory)src).GetEntries().TrueForAll(entry => copyHelper(entry, newDir, false));
            }
            else
            {
                // file: copy
                VfsFile file = (VfsFile)src;
                file.Duplicate(dst, newName);
                return true;
            }
        }

        public static void navigateUp()
        {
            
            if (workingDirectory.Parent != null)
                workingDirectory = workingDirectory.Parent;
            
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
            //Get disk and parent directory:
            VfsDirectory parent;
            IEnumerable<string> remaining;
            getEntry(dst, out parent, out remaining);
            var disk = parent.Disk;
            //Get reader & writer
            var reader = disk.getReader();
            var writer = disk.getWriter();
            
            //Get fileName
            var file = (VfsFile) getEntry(dst);
            var fileName = file.Name;

            //check if there's already a file with that name
            if (parent.GetFiles().SkipWhile(e => !e.Name.Equals(fileName)).Count() != 0)
                throw new ArgumentException("this directory already has a file with the name " + fileName);
            //Check if file exists at src
            if (!File.Exists(src))
                throw new Exception("file does not exist at " + src);

            //Get FileLength
            var fileInfo = new FileInfo(src);
            //TODO: Might lose precision...
            var fileLength = Convert.ToInt32(fileInfo.Length);
            //File in which to write
            var importEntry = EntryFactory.createFile(disk, fileName, fileLength, parent);

            importEntry.Write(new BinaryReader(fileInfo.OpenRead()));
            parent.AddElement(importEntry);
        }

        /// <summary>
        /// Writes a VfsFile to the location indicated by dst in the host-file from src
        /// </summary>
        /// <param name="dst">The absolute path to the target Directory (Host file system).</param>
        /// <param name="src">The absolute path to the File that should be exported.</param>
        public static void Export(string dst, string src) {
            if (dst == null) throw new ArgumentNullException("dst");
            if (src == null) throw new ArgumentNullException("src");
            
            //Get the file to export and its name
            var toExport = (VfsFile) getEntry(src);
            if (toExport == null) 
                throw new NullReferenceException("The file to export is null.");
            var fileName = toExport.Name;
           if (fileName == null)
               throw new NullReferenceException("Name of file is null.");
            
            //Create the path to destination (non existent folders are automatically created)
            System.IO.Directory.CreateDirectory(dst);
            
            //Get path including fileName
            //TODO: What about those extensions?
            var completePath = System.IO.Path.Combine(dst, fileName);

            //Check if file with same name already exists at that location
            if (System.IO.File.Exists(completePath))
                throw new Exception("There's already a file with the same name in the host file system");

            //Start actual export
            var fs = System.IO.File.Create(completePath);
            var writer = new BinaryWriter(fs);
            toExport.Read(writer);
            
            //Remove file from entries list of parent directory
            toExport.Parent.GetEntries().Remove(toExport);
            
            //Close and dispose resources
            writer.Dispose();
            fs.Dispose();
            writer.Close();
            fs.Close();
        }
        
        /// <summary>
        /// Gets parent directory from a file path
        /// </summary>
        /// <param name="arg">The path to the file</param>
        public static String GetParentNameFromPath(string arg) 
        {
            var startIndexFileName = arg.LastIndexOf('/') + 1;
            int i = startIndexFileName - 2; char tmp;
            do {
                tmp = arg[i];
                i--; //will be one position before secondLastSlash --> +2 for first char of parent
            } while (!tmp.Equals('/'));
            return arg.Substring(i + 2, startIndexFileName - 2); //-2 to be at last char before last slash
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
            var entry = getEntry(CurrentDisk, workingDirectory.GetAbsolutePath() + "/" + ident) as VfsFile;

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
                return _disks.Single(x => x.DiskProperties.Name == diskName);
            }
            throw new DiskNotFoundException();
        }
        #endregion
        public static void GetFreeSpace()
        {
            var divisor = 1;
            while ((CurrentDisk.DiskProperties.NumberOfBlocks - CurrentDisk.DiskProperties.NumberOfUsedBlocks)*
                   CurrentDisk.DiskProperties.BlockSize/((int) Math.Pow(1024, divisor - 1)) > 1024)
            {
                divisor++;
            }
            Console.Message(((CurrentDisk.DiskProperties.NumberOfBlocks - CurrentDisk.DiskProperties.NumberOfUsedBlocks)*
                            CurrentDisk.DiskProperties.BlockSize/(Math.Pow(1024, divisor - 1))).ToString("#.##") + idToSize[divisor - 1] + " free space available on " +
                            CurrentDisk.DiskProperties.Name);
        }

        public static void GetOccupiedSpace()
        {
            var divisor = 1;
            while (CurrentDisk.DiskProperties.NumberOfUsedBlocks*CurrentDisk.DiskProperties.BlockSize/
                   ((int) Math.Pow(1024, divisor - 1)) > 1024)
            {
                divisor++;
            }
            Console.Message((CurrentDisk.DiskProperties.NumberOfUsedBlocks*
                            CurrentDisk.DiskProperties.BlockSize/(Math.Pow(1024, divisor - 1))).ToString("#.##") + idToSize[divisor - 1] + " space is occupied on " +
                            CurrentDisk.DiskProperties.Name);
        }

        public static void Exit()
        {
            foreach (var vfsDisk in _disks)
            {
                UnloadDisk(vfsDisk.DiskProperties.Name);
            }
        }
    }
}
