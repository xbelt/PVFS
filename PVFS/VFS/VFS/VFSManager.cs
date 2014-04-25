using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VFS.VFS.Models;
using VFS.VFS.Parser;

namespace VFS.VFS
{
    public class VfsManager
    {
        private readonly static List<VfsDisk> Disks = new List<VfsDisk>();
        public static VfsDirectory WorkingDirectory { get; private set; }
        public static VfsDisk CurrentDisk { get; private set; }
        public static VfsConsole Console { get; set; }
        private readonly static string[] IdToSize = { "bytes", "kb", "mb", "gb", "tb" };
        private const string Salt = "d5fg4df5sg4ds5fg45sdfg4";
        private const int SizeOfBuffer = 1024 * 8;

        static VfsManager()
        {
            Console = new VfsConsole();
        }

        //----------------------Private Methods----------------------

        /// <summary>
        /// Returns the disk with the corresponding name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns a VfsDisk if it was found, otherwise null.</returns>
        private static VfsDisk GetDisk(string name)
        {
            lock (Disks)
            {
                return Disks.FirstOrDefault(d => d.DiskProperties.Name.Equals(name));
            }
        }

        /// <summary>
        /// Returns the corresponding VfsEntry
        /// </summary>
        /// <param name="path">A path containing the disk name (can be relative or absolute)</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        public static VfsEntry GetEntry(string path)
        {
            VfsDirectory last;
            IEnumerable<string> remaining;
            return GetEntry(path, out last, out remaining);
        }

        /// <summary>
        /// Returns the corresponding VfsEntry
        /// </summary>
        /// <param name="path">A path containing the disk name (can be relative or absolute)</param>
        /// <param name="last">Returns the last found directory (usefull if not the whole path exists). Can be root. Null if disk was not found.</param>
        /// <param name="remaining">Returns the remaining path, if the result was null.</param>
        /// <returns>Returns the entry if found, otherwise null. Can be root.</returns>
        public static VfsEntry GetEntry(string path, out VfsDirectory last, out IEnumerable<string> remaining)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            //Check if path is relative, if so, make it absolute
            path = GetAbsolutePath(path);

            int i = path.IndexOf('/', 1);
            if (i == -1)
            {
                var diskRoot = GetDisk(path.Substring(1));
                if (diskRoot == null)
                {
                    last = null;
                    remaining = new List<string>{path};
                    return null;
                }
                last = null;
                remaining = new List<string>();
                return diskRoot.Root;
            }

            var disk = GetDisk(path.Substring(1, i - 1));
            if (disk != null)
                return GetEntry(disk, path.Substring(i + 1), out last, out remaining);
            
            last = null;
            remaining = path.Split(new[] { '/' }, StringSplitOptions.None);
            return null;
        }

        /// <summary>
        /// Returns the corresponding VfsEntry. 
        /// </summary>
        /// <param name="disk">The disk to start on.</param>
        /// <param name="path">An absolute path, not containing the disk name</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        public static VfsEntry GetEntry(VfsDisk disk, string path)
        {
            VfsDirectory last;
            IEnumerable<string> remaining;
            return GetEntry(disk, path, out last, out remaining);
        }

        /// <summary>
        /// Returns the corresponding VfsEntry.
        /// </summary>
        /// <param name="disk">The disk to start on.</param>
        /// <param name="path">An absolute path, not containing the disk name</param>
        /// <param name="last">Returns the last found directory (usefull if not the whole path exists). Can be root.</param>
        /// <param name="remaining">Returns the remaining path, if the result was null.</param>
        /// <returns>Returns the entry if found, otherwise null. Can be root.</returns>
        public static VfsEntry GetEntry(VfsDisk disk, string path, out VfsDirectory last, out IEnumerable<string> remaining)
        {
            if (disk == null) throw new ArgumentNullException("disk");
            if (path == null)
                throw new ArgumentNullException("path");

            VfsDirectory current = disk.Root;
            string[] names = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 0)
            {
                last = null;
                remaining = names;
                return current;
            }
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
        private static string GetTempFilePath()
        {
            string path;
            int i = 0;
            do
            {
                path = Environment.CurrentDirectory + "\\temp" + i++;
            } while (File.Exists(path));
            return path;
        }

        //----------------------Concurrent----------------------

        /// <summary>
        /// Looks for a VfsEntry concurrently.
        /// Does not load any entries and therefore not move any readers/writers of the disk.
        /// Can be used concurrently to other operations.
        /// </summary>
        /// <param name="path">The path to look for.</param>
        /// <param name="entry">The VfsEntry if found, null otherwise</param>
        /// <returns>0 if it was ok, 1 if this path does not exist, 2 if this entry can't be loaded concurrently</returns>
        public static int GetEntryConcurrent(string path, out VfsEntry entry)
        {
            path = GetAbsolutePath(path);

            int i = path.IndexOf('/', 1);
            if (i == -1)
            {
                var diskRoot = GetDisk(path.Substring(1));
                if (diskRoot == null)
                {
                    entry = null;
                    return 1;
                }
                entry = diskRoot.Root;
                return 0;
            }

            var disk = GetDisk(path.Substring(1, i - 1));
            if (disk != null)
            {
                VfsDirectory current = disk.Root;
                string[] names = path.Substring(i + 1).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length == 0)
                {
                    entry = current;
                    return 0;
                }
                for (int j = 0; j < names.Length - 1; j++)
                {
                    if (!current.IsLoaded)
                    {
                        entry = null;
                        return 2;
                    }
                    current = current.GetDirectory(names[j]);
                    if (current == null)
                    {
                        entry = null;
                        return 1; // not found
                    }
                }
                if (!current.IsLoaded)
                {
                    entry = null;
                    return 2;
                }
                entry = current.GetEntry(names.Last());
                return 0;
            }
            else
            {
                entry = null;
                return 1;
            }
        }

        /// <summary>
        /// Returns a list of all directories and files contained in a specified target directory concurrently.
        /// This is a readonly operation and can be run while other operations are executing paralelly.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="directories">The subdirectories of that directory.</param>
        /// <param name="files">The files of that directory.</param>
        /// <returns>0 if it was ok, 1 if this path does not exist, 2 if this operation can't be done concurrently</returns>
        public static int ListEntriesConcurrent(string path, out List<string> directories, out List<string> files)
        {
            if (Disks.Count == 0)
            {
                directories = null;
                files = null;
                return 1;
            }

            VfsEntry entry;
            int res = GetEntryConcurrent(path, out entry);

            if (res != 0)
            {
                directories = null;
                files = null;
                return res;
            }


            if (entry == null)
            {
                directories = null;
                files = null;
                return 1;
            }

            if (!entry.IsDirectory)
            {
                directories = null;
                files = null;
                return 1;
            }


            VfsDirectory dir = (VfsDirectory)entry;

            if (!dir.IsLoaded)
            {
                directories = null;
                files = null;
                return 2;
            }

            directories = dir.GetDirectories.Select(d => d.Name).ToList();
            files = dir.GetFiles.Select(d => d.Name).ToList();
            return 0;
        }


        //----------------------Command----------------------

        /// <summary>
        /// Executes a commandline comand.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public static void ExecuteCommand(string command)
        {
            var input = new AntlrInputStream(command);
            var lexer = new ShellLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new ShellParser(tokens);

            var entry = parser.compileUnit();

            var walker = new ParseTreeWalker();
            var exec = new Executor();
            walker.Walk(exec, entry);
        }

        //----------------------Helper----------------------

        /// <summary>
        /// Returns the absolute path for the supplied path/identifier.
        /// For identifier: workingDirectory/ident
        /// Supports . and ..
        /// </summary>
        /// <param name="path">The path or identifier.</param>
        /// <returns>A valid absolute path starting with /.</returns>
        public static string GetAbsolutePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path == "." || String.IsNullOrEmpty(path))
                return WorkingDirectory.AbsolutePath;
            if (path == "..")
                return (WorkingDirectory.Parent ?? WorkingDirectory).AbsolutePath;
            if (path.StartsWith("/"))
                return path;
            return WorkingDirectory.AbsolutePath + "/" + path;
        }


        //----------------------Disk----------------------

        /// <summary>
        /// Lists the currently loaded disks.
        /// </summary>
        public static void ListDisks()
        {
            Console.Message(Disks.Select(d => d.DiskProperties.Name).Concat(" "));
        }

        public static void LoadDisk(VfsDisk disk)
        {
            if (disk == null) throw new ArgumentNullException("disk");

            if (GetDisk(disk.DiskProperties.Name) != null)
            {
                Console.ErrorMessage("A disk with the same name was already in the VFS.");
            }
            lock (Disks)
            {
                Disks.Add(disk);
            }
            CurrentDisk = disk;
            WorkingDirectory = disk.Root;

            Console.Message("Opened disk " + disk.DiskProperties.Name + ".");
        }
        
        public static bool UnloadDisk(string name)
        {
            VfsDisk disk = GetDisk(name);

            if (disk == null)
            {
                Console.ErrorMessage("This disk does not exist.");
                return false;
            }

            Disks.Remove(disk);
            if (CurrentDisk == disk)
            {
                CurrentDisk = Disks.FirstOrDefault();
                WorkingDirectory = CurrentDisk == null ? null : CurrentDisk.Root;
            }
            disk.Dispose();
            Console.Message("Closed disk " + disk.DiskProperties.Name + ".");
            return true;
        }

        public static void CreateDisk(string path, string name, double size, int blockSize, string pw)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (name == null) throw new ArgumentNullException("name");
            if (pw == null) throw new ArgumentNullException("pw");
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            if (!VfsFile.ValidName(name))
            {
                Console.ErrorMessage("This disk name is not valid.");
                return;
            }

            if (Disks.Any(d => d.DiskProperties.Name == name))
            {
                Console.ErrorMessage("There is already a disk with that name in the VFS.");
                return;
            }

            if (!Directory.Exists(path))
            {
                Console.ErrorMessage("Directory does not exist.");
                return;
            }
            if (File.Exists(path + name + ".vdi"))
            {
                Console.ErrorMessage("This disk already exists");
                return;
            }
            if (blockSize%4 != 0 || blockSize <= FileOffset.Header)
            {
                Console.ErrorMessage("Blocksize must be a multiple of 4 and larger than 128!");
                return;
            }
            if (size < blockSize || size/blockSize < 5)
            {
                Console.ErrorMessage("The disk must at least hold 5 blocks.");
                return;
            }
            var disk = DiskFactory.Create(new DiskInfo(path, name, size, blockSize), pw);
            LoadDisk(disk);
        }

        public static void RemoveDisk(string path)
        {
            if (!File.Exists(path))
            {
                Console.ErrorMessage("This disk does not exist.");
                return;
            }

            DiskFactory.Remove(path);
        }

        //----------------------Working Directory----------------------

        /// <summary>
        /// Changes the working directory to a new one by path.
        /// </summary>
        /// <param name="path">The path to the new working directory.</param>
        public static void ChangeWorkingDirectory(string path)
        {
            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            VfsEntry entry = GetEntry(path);

            if (entry == null || !entry.IsDirectory)
            {
                Console.ErrorMessage("This path does not exist.");
                return;
            }

            WorkingDirectory = (VfsDirectory) entry;
            CurrentDisk = WorkingDirectory.Disk;

            Console.Message("New working directory: " + path);
        }

        public static void NavigateUp()
        {
            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            if (WorkingDirectory.Parent != null)
                WorkingDirectory = WorkingDirectory.Parent;

            Console.Message("New working directory: " + WorkingDirectory.AbsolutePath);
        }

        //----------------------Directory and File----------------------

        /// <summary>
        /// Writes the names of all subfiles and subdirectories of a given directory into the console.
        /// </summary>
        /// <param name="path">Target directory.</param>
        /// <param name="files">Display files.</param>
        /// <param name="dirs">Display directories.</param>
        public static void ListEntries(string path, bool files, bool dirs)
        {
            if (path == null) throw new ArgumentNullException("path");

            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            VfsEntry entry = GetEntry(path);

            if (entry == null)
            {
                Console.ErrorMessage("Directory not found.");
                return;
            }

            if (!entry.IsDirectory)
            {
                Console.ErrorMessage("This is not a directory.");
                return;
            }

            VfsDirectory dir = (VfsDirectory)entry;

            var dirList = dir.GetDirectories.Select(x => x.Name).ToList();
            var fileList = dir.GetFiles.Select(x => x.Name).ToList();

            string output = "";



            if (dirs)
                output += dirList.Concat(" ");
            if (dirs && files)
                output += "\n";
            if (files)
                output += fileList.Concat(" ");

            Console.Message(output);
        }
        
        /// <summary>
        /// Creates directories such that the whole path given exists. If the path contains a file this does nothing.
        /// </summary>
        /// <param name="path">The path to create.</param>
        /// <param name="silent">Indicates whether there should be an output into the console if the operation was successful.</param>
        /// <returns>Returns true if the operation succeded, otherwise false.</returns>
        public static bool CreateDirectory(string path, bool silent)
        {
            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return false;
            }

            VfsDirectory last;
            IEnumerable<string> remainingPath;
            VfsEntry entry = GetEntry(path, out last, out remainingPath);

            List<string> remaining = remainingPath.ToList();

            if (entry == null)
            {
                if (last == null)
                {
                    if (remaining.Any())
                        Console.ErrorMessage("The Disk " + remaining.First() + " does not exist!");
                    else
                        Console.ErrorMessage("Please enter a valid path.");
                    return false;
                }

                foreach (string name in remaining)
                {
                    VfsEntry next = last.GetEntry(name);
                    if (next == null)
                    {
                        // create
                        if (!VfsFile.ValidName(name))
                        {
                            Console.ErrorMessage("The name of the directory was not valid.");
                            return false;
                        }
                        if (last.SpaceIndicator(1) + 1 > last.Disk.GetFreeBlocks)
                        {
                            Console.ErrorMessage("The disk has not enough space to create a new directory.");
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
                        Console.ErrorMessage("A file with the same name already existed.");
                        return false;
                    }
                }
                if (!silent)
                    Console.Message("Created the path " + path + ".");
                return true;
            }
            if (entry.IsDirectory)
            {
                if (!silent)
                    Console.Message("This Directory already existed.");
                return true;
            }
            Console.ErrorMessage("A file with the same name already existed.");
            return false;
        }

        /// <summary>
        /// Creates an empty file at the indicated location. Returns early if the target directory doesn't exist.
        /// </summary>
        /// <param name="path">The path to the new file.</param>
        public static VfsFile CreateFile(string path)
        {
            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return null;
            }

            VfsDirectory last;
            IEnumerable<string> remaining;
            VfsEntry entry = GetEntry(path, out last, out remaining);

            if (entry != null)
            {
                Console.ErrorMessage("There already existed a file or directory with the same path.");
                return null;
            }

            if (last == null)
            {
                Console.ErrorMessage("The disk was not found.");
                return null;
            }

            List<string> fileNames =  remaining.ToList();
            if (fileNames.Count != 1)
            {
                Console.ErrorMessage("Target directory was not found.");
                return null;
            }

            if (!VfsFile.ValidName(fileNames[0]))
            {
                Console.ErrorMessage("This filename is not valid");
                return null;
            }

            if (last.SpaceIndicator(1) + 1 > last.Disk.GetFreeBlocks)
            {
                Console.ErrorMessage("The disk has not enough space to create a new file.");
                return null;
            }

            VfsFile file = EntryFactory.createFile(last.Disk, fileNames[0], 0, last);
            last.AddElement(file);

            Console.Message("File successfully created.");

            return file;
        }

        /// <summary>
        /// Moves a File or Directory to a new location. A directory is moved recursively.
        /// </summary>
        /// <param name="srcPath">The absolute path to the File/Directory that should be moved.</param>
        /// <param name="dstPath">The absolute path to the target Directory.</param>
        public static void Move(string srcPath, string dstPath)
        {
            if (srcPath == null)
                throw new ArgumentNullException("srcPath");
            if (dstPath == null)
                throw new ArgumentNullException("dstPath");

            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            VfsEntry srcEntry = GetEntry(srcPath);
            VfsEntry dstEntry = GetEntry(dstPath);

            if (srcEntry == null || dstEntry == null)
            {
                Console.ErrorMessage("The source or the destination did not exist.");
                return;
            }
            if (!dstEntry.IsDirectory)
            {
                Console.ErrorMessage("The destination must be a directory.");
                return;
            }

            VfsFile src = (VfsFile)srcEntry;
            VfsDirectory dst = (VfsDirectory)dstEntry;


            if (src.Parent == null)
            {
                Console.ErrorMessage("Can't move root.");
                return;
            }
            if (dst.AbsolutePath.StartsWith(src.AbsolutePath))
            {
                Console.ErrorMessage("Can't move a directory into itself!");
                return;
            }

            if (dst.GetEntry(src.Name) != null)
            {
                Console.ErrorMessage("There is already a file with that name in the target directory.");
                return;
            }

            if (src.Disk != dst.Disk)
            {
                if (dst.SpaceIndicator(1) + VfsFile.GetNoBlocks(dst.Disk, src.FileSize) > dst.Disk.GetFreeBlocks)
                {
                    Console.ErrorMessage("The disk has not enough space to move this file.");
                    return;
                }
                string tmp = GetTempFilePath();

                Export(srcPath, tmp);
                Import(tmp, dstPath);
                Remove(srcPath);
                File.Delete(tmp);
            }
            else
            {
                if (dst.SpaceIndicator(1) > dst.Disk.GetFreeBlocks)
                {
                    Console.ErrorMessage("The disk has not enough space to move this file.");
                    return;
                }
                src.Parent.RemoveElement(src);
                dst.AddElement(src);
                src.Parent = dst;
                src.UpdateFileHeader();
            }

            Console.Message("Moved  " + srcPath + " to " + dstPath + ".");
        }
        
        /// <summary>
        /// Renames a file or directory.
        /// </summary>
        /// <param name="src">The path to the file/directory.</param>
        /// <param name="newName">The new name.</param>
        public static void Rename(string src, string newName)
        {
            if (src == null) throw new ArgumentNullException("src");
            if (newName == null) throw new ArgumentNullException("newName");

            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            if (!VfsFile.ValidName(newName))
            {
                Console.ErrorMessage("This new name is not valid.");
                return;
            }

            VfsDirectory last;
            IEnumerable<string> remaining;
            VfsEntry entry = GetEntry(src, out last, out remaining);

            if (entry == null)
            {
                Console.ErrorMessage("This file does not exist.");
                return;
            }

            if (((VfsFile) entry).Parent == null)
            {
                Console.ErrorMessage("Can't rename root.");
                return;
            }

            if (last.GetEntry(newName) != null)
            {
                Console.ErrorMessage("There already exists a file or directory with this name.");
                return;
            }

            ((VfsFile)entry).Rename(newName);

            Console.Message("Renamed file to " + newName + ".");
        }

        /// <summary>
        /// Copies a file/directory into a target directory. Directories will be copied recursively.
        /// If the target directory does not exist it can be created.
        /// </summary>
        /// <param name="srcPath">The path to the file/directory which should be copied.</param>
        /// <param name="dstPath">The target directory.</param>
        public static void Copy(string srcPath, string dstPath)
        {
            if (srcPath == null)
                throw new ArgumentNullException("srcPath");
            if (dstPath == null)
                throw new ArgumentNullException("dstPath");

            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            VfsEntry srcEntry = GetEntry(srcPath);
            VfsEntry dstEntry = GetEntry(dstPath);

            if (srcEntry == null)
            {
                Console.ErrorMessage("Source did not exist.");
                return;
            }
            if (dstEntry != null && !dstEntry.IsDirectory)
            {
                Console.ErrorMessage("Destination must be a directory.");
                return;
            }

            if (dstEntry == null)
            {
                if (Console.Query("The destination directory did not exist. Do you want to create it?", "Yes", "Cancel") == 0)
                    CreateDirectory(dstPath, true);
                dstEntry = GetEntry(dstPath);
                if (dstEntry == null)
                {
                    Console.ErrorMessage("The destination directory did not exist.");
                    return;
                }
            }
            VfsDirectory dst = (VfsDirectory)dstEntry;
            if (((VfsFile) srcEntry).Disk != dst.Disk)
            {
                Console.ErrorMessage("Can't copy to anothe disk.");
                return;
            }

            if (!CopyHelper(srcEntry, dst, ((VfsFile)srcEntry).Parent == dst))
            {
                Console.ErrorMessage("Copying was canceled due to a invalid file or directory name or not enough space on the disk.");
                return;
            }

            Console.Message("Copied " + srcPath + " to " + dstPath + ".");
        }

        /// <summary>
        /// A recursive helper function for Copy().
        /// </summary>
        /// <param name="src">The file/dir to copy.</param>
        /// <param name="dst">The target directory.</param>
        /// <param name="first">Is this the first invocation of the recursive call?</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private static bool CopyHelper(VfsEntry src, VfsDirectory dst, bool first)
        {
            // find name for copy
            string newName = src.Name + (first ? "_copy" : "");
            int i = 2;
            while (dst.GetEntry(newName) != null)
                newName = src.Name + "_copy_" + i++;

            if (!VfsFile.ValidName(newName))
                return false;

            // check if file or dir
            if (src.IsDirectory)
            {
                // dir: create, recursive calls
                if (dst.SpaceIndicator(1) + 1 > dst.Disk.GetFreeBlocks)
                    return false;
                VfsDirectory newDir = EntryFactory.createDirectory(dst.Disk, newName, dst);
                dst.AddElement(newDir);
                return ((VfsDirectory)src).GetEntries.ToList().TrueForAll(entry => CopyHelper(entry, newDir, false));
            }
            else
            {
                // file: copy
                VfsFile file = (VfsFile)src;
                if (dst.SpaceIndicator(1) + VfsFile.GetNoBlocks(dst.Disk, file.FileSize) > dst.Disk.GetFreeBlocks)
                    return false;
                file.Duplicate(dst, newName);
                return true;
            }
        }

        /// <summary>
        /// Deletes a file or directory from the disk.
        /// </summary>
        /// <param name="path">THe path to the file/directory.</param>
        public static void Remove(string path)
        {
            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            var entry = GetEntry(path) as VfsFile;

            if (entry == null)
            {
                Console.ErrorMessage("This file does not exist.");
                return;
            }

            if (entry.Parent == null)
            {
                Console.Message("Can't remove root directory.");
                return;
            }

            if (entry.IsDirectory)
            {
                var files = ((VfsDirectory)entry).GetFiles.ToList();
                foreach (var file in files)
                {
                    file.Free();
                }
                var directories = ((VfsDirectory)entry).GetDirectories.ToList();
                foreach (var directory in directories)
                {
                    Remove(directory.AbsolutePath);
                }
            }
            entry.Parent.RemoveElement(entry);
            entry.Free();

            Console.Message("Successfully removed " + entry.Name + ".");
        }


        //----------------------Import and Export----------------------

        private static string Compress(FileInfo fileToCompress) {
            String returnPath;
            long savedBytes = 0;
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if (( File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden ) == FileAttributes.Hidden & fileToCompress.Extension != ".gz") 
                    return fileToCompress.FullName; //Was an additional condition: 
                using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                {
                    returnPath = fileToCompress.FullName + ".gz";
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                        savedBytes += fileToCompress.Length - compressedFileStream.Length;
                    }
                }
            }
            Console.Message("You saved " + savedBytes + " bytes thanks to compression.");
            return returnPath;
        }

        private static string Decompress(FileInfo fileToDecompress)
        {
            string returnPath;
            if (fileToDecompress == null) throw new ArgumentNullException("fileToDecompress");
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                var currentFileName = fileToDecompress.FullName;
                var newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);
                returnPath = newFileName;
                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
            return returnPath;
        }

        /// <summary>
        /// Imports a File from the host Filesystem to a directory inside the virtual Filesystem. (are we supporting importing whole directories too?)
        /// Overwrites already existing files with the same name.
        /// Creates the target directory if it doesn't exist. Introduces grandpa Joe.
        /// 
        /// Throws an exception if you try to import the virtual disk itself!
        /// </summary>
        /// <param name="src">The absolute path to the File (or Directory) that should be imported (Host file system).</param>
        /// <param name="dst">The (not necesseraly) absolute path to the directory where we import.</param>
        public static void Import(string src, string dst) 
        {
            if (src == null) throw new ArgumentNullException("src");
            if (dst == null) throw new ArgumentNullException("dst");

            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            VfsEntry dstEntry = GetEntry(dst);

            if (dstEntry == null)
            {
                Console.ErrorMessage("The destination folder did not exist.");
                return;
            }

            if (!dstEntry.IsDirectory)
            {
                Console.ErrorMessage("Destination was a file.");
                return;
            }

            VfsDirectory dstDir = (VfsDirectory) dstEntry;

            if (Disks.Any(d=>d.Path == src))
            {
                Console.ErrorMessage("Can't import a currently opened disk.");
                return;
            }

            //Check if source is valid: TODO: also check file and dir names
            if (File.Exists(src))
            {
                ImportFile(src, dstDir);
            }
            else if (Directory.Exists(src))
            {
                ImportDirectory(src, dstDir);
            }
            else
            {
                Console.ErrorMessage("Your source path does not lead to a valid file or directory. Aborting import operation.");
                return;
            }
            Console.Message("Finished importing.");
        }

        private static string EncryptFile(FileInfo src, String password)
        {
            var srcPath = src.FullName;
            var dstPath = srcPath + ".enc";

            if (password == null)
            {
                if (!File.Exists(dstPath))
                    File.Copy(srcPath, dstPath);
                return dstPath;
            }

            var input = new FileStream(srcPath, FileMode.Open, FileAccess.Read);
            var output = new FileStream(dstPath, FileMode.OpenOrCreate, FileAccess.Write);

            var algorithm = new RijndaelManaged { KeySize = 256, BlockSize = 128 };
            var key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(Salt));

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            using (var encryptedStream = new CryptoStream(output, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
            {
                CopyStream(input, encryptedStream);
            }

            return dstPath;
        }

        private static string DecryptFile(FileInfo src, String password)
        {
            var inputPath = src.FullName;
            var outputPath = inputPath + ".dc";
            if (password == null)
            {
                return outputPath;
            }
            var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
            var output = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write);
            
            var algorithm = new RijndaelManaged { KeySize = 256, BlockSize = 128 };
            var key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(Salt));

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            try
            {
                using (var decryptedStream = new CryptoStream(output, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    CopyStream(input, decryptedStream);
                    File.Delete(inputPath);
                    File.Move(outputPath, inputPath);
                }
            }
            catch (CryptographicException)
            {
                throw new InvalidDataException("Please supply the correct password");
            }
            return outputPath;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            using (output)
            using (input)
            {
                byte[] buffer = new byte[SizeOfBuffer];
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, read);
                }
            }
        }

        private static void ImportFile(string src, VfsDirectory dstDir) 
        {   
            var toEncrypt = new FileInfo(src);
            //Encrypt file with disk password
            var encryptedFile = EncryptFile(toEncrypt, dstDir.Disk.Password);
            Console.Message("Finished encryption");
            //Get FileInfo
            var toCompress = new FileInfo(encryptedFile);

            Console.Message("Starting compression");
            //Compress the file before importing
            var watch = new Stopwatch();
            watch.Start();
            var compressedSrc = Compress(toCompress);
            watch.Stop();
            Console.Message("Compression finished after: " + watch.Elapsed);

            //get the compressed file and its name
            var fileInfo = new FileInfo(compressedSrc);
            var fileName = fileInfo.Name.Remove(fileInfo.Name.Length - 3);
            if (fileName.EndsWith(".enc"))
            {
                fileName = fileName.Remove(fileName.Length - 4);
            }

            //Get fileLength
            var fileLengthLong = fileInfo.Length + FileOffset.Header - FileOffset.SmallHeader;
            if (fileLengthLong > ((long)dstDir.Disk.DiskProperties.BlockSize - (long)FileOffset.SmallHeader) * ((long)dstDir.Disk.DiskProperties.NumberOfBlocks - (long)dstDir.Disk.DiskProperties.NumberOfUsedBlocks))
            {
                Console.ErrorMessage("Filesize too large. Skipping import of: " + fileInfo.Name);
                return;
            }
            var fileLength = fileInfo.Length;

            Console.Message("Importing " + fileName + " in " + dstDir.AbsolutePath);

            if (!VfsFile.ValidName(fileName))
            {
                fileName = new String(fileName.Where(c => Char.IsLetterOrDigit(c) || c == '_' || c == '.').ToArray());
            }
            //Check for duplicates
            var fileWithSamename = dstDir.GetFile(fileName);
            if (fileWithSamename != null)
            {
                var answer = Console.Query("There is already a file with this " + fileName + ", do you want to overwrite it?", "Ok", "Cancel");
                if (answer == 1)
                {
                    Console.ErrorMessage("File has not been overwritten.");
                    return;
                }
                //Delete file to create a new one.
                Console.Message("Removing " + fileWithSamename.Name);
                Remove(fileWithSamename.AbsolutePath); //Works because of duplicate name.
            }

            //Create entry in which to write the file
            var importEntry = EntryFactory.createFile(dstDir.Disk, fileName, fileLength, dstDir);

            //Start actual import
            var reader = new BinaryReader(fileInfo.OpenRead());
            importEntry.Write(reader);

            //Dispose resources and close reader
            reader.Close();

            //Delete the compressed file in host system (what kind of compression would it be to have the same file twice? :P)
            File.Delete(compressedSrc);
            File.Delete(encryptedFile);
        }

        private static void ImportDirectory(string src, VfsDirectory dstDir) 
        {
            //Name of directory to write
            var dirInfo = new DirectoryInfo(src);
            var dirName = dirInfo.Name;
          //  dirName = MakeStringValid(dirName); //TODO: is it okay to just change it without asking?
            //Check for duplicates
            var dirWithSameName = dstDir.GetDirectory(dirName);
            if (dirWithSameName != null)
            {
                Console.Message("There is already a directory with the name: " + dirName + ".");
                var answer = Console.Query(
                    "Do you want to overwrite it and its content? Write 'Ok' or 'Cancel'. ", "Ok","Cancel");
                if (answer == 1)
                {
                    Console.Message("Directory has not been imported.");
                    return;
                }
                //Delete directory and content to create a new one
                Remove(dirWithSameName.AbsolutePath); //Work because of duplicate name
            }

            //Create directory
            Console.Message("Importing " + dirName + " in " + dstDir.AbsolutePath);
            var newDir = EntryFactory.createDirectory(dstDir.Disk, dirName, dstDir);
            
            //If src contains files or subdirectories, we have to import those into newDir:
            var subFiles = Directory.GetFiles(src).ToList();
            var subDirs  = Directory.GetDirectories(src).ToList();

            if (subFiles.Count > 0)
            {
                foreach (var subFile in subFiles)
                {
                    ImportFile(subFile, newDir);
                }
            }

            //If there are no subDirs, we're done.
            if (subDirs.Count == 0) return;

            foreach (var subDir in subDirs)
            {
                ImportDirectory(subDir, newDir);
            }
        }

        /// <summary>
        /// Writes a VfsFile to the location indicated by dst in the host-file from src
        /// </summary>
        /// <param name="dst">The absolute path to the target Directory (Host file system).</param>
        /// <param name="src">The absolute path to the target Entry that should be exported.</param>
        public static void Export(string src, string dst)
        {   
            if (dst == null) throw new ArgumentNullException("dst");
            if (src == null) throw new ArgumentNullException("src");

            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            if (!Directory.Exists(dst))
            {
                Console.ErrorMessage("Destination does not lead to a directory: " + dst);
                return;
            }

            //get entry to export
            var toExport = GetEntry(src);
            if (toExport == null)
            {
                Console.ErrorMessage("Invalid path to source entry: " + src);
                return;
            }

            if (toExport.IsDirectory)
            {
                ExportDirectory(dst, (VfsDirectory) toExport, true);
            }
            else
            {
                ExportFile(dst, (VfsFile) toExport);
            }
        }

        private static void ExportFile(string dst, VfsFile toExport) //TODO: finish this thing here
        {
            //Get the entry to export and its name
            if (toExport == null) 
                throw new ArgumentNullException("toExport");
            var entryName = toExport.Name;
            if (entryName == null) //TODO: probably useless check
               throw new NoNullAllowedException("entry name must not be null");
            
            //Create the path to destination (non existent folders are automatically created)
            Directory.CreateDirectory(dst);
            
            //Get path including fileName
            var completePath = Path.Combine(dst, entryName) + ".gz";

            //Check if file with same name already exists at that location
            if (File.Exists(completePath))
            {
                Console.Message("There's already a file with the same name in the host file system");
                return;
            }

            //Start actual export
            var fs = File.Create(completePath);
            var writer = new BinaryWriter(fs);
            toExport.Read(writer);
            
            //Close and dispose resources
            writer.Close();
           
            //Decompress file
            var toDecompress = new FileInfo(completePath);
            var decompressed = Decompress(toDecompress);

            var toDecrypt = new FileInfo(decompressed);
            var decryptionTempFile = DecryptFile(toDecrypt, toExport.Disk.Password);

            //Delete compressed file
            File.Delete(toDecompress.FullName);
            File.Delete(decryptionTempFile);
        }

        private static void ExportDirectory(string dst, VfsDirectory toExport, bool isFirstRecursion) 
        {
            var filesInDir = toExport.GetFiles.ToList();
            var subDirs = toExport.GetDirectories.ToList();
            var path = dst;
            //Export directory
            //TODO: check for correctness
            if (isFirstRecursion) 
                path += "\\" + toExport.Name;

            Directory.CreateDirectory(path);

            //Export the files in 'toExport'
            if (filesInDir.Count != 0)
            {
                foreach (var file in filesInDir)
                {
                    ExportFile(path, file);
                }               
            }
            //If there are no subfolders in toExport: we're done
            if (subDirs.Count == 0) return;
            
            //If there are subfolders in toExport: export them
            foreach (var subDir in subDirs)
            {
                ExportDirectory(path + "\\" + subDir.Name, subDir, false);
            }
        }

        //----------------------Disk Properties----------------------

        public static double GetFreeSpace()
        {
            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return 0d;
            }

            var divisor = 1;
            while ((CurrentDisk.DiskProperties.NumberOfBlocks - CurrentDisk.DiskProperties.NumberOfUsedBlocks)*
                   CurrentDisk.DiskProperties.BlockSize/((int) Math.Pow(1024, divisor - 1)) > 1024)
            {
                divisor++;
            }
            var result = ((CurrentDisk.DiskProperties.NumberOfBlocks - CurrentDisk.DiskProperties.NumberOfUsedBlocks)*
                          CurrentDisk.DiskProperties.BlockSize/(Math.Pow(1024, divisor - 1)));
            Console.Message(result.ToString("0.##") + IdToSize[divisor - 1] + " free space available on " +
                            CurrentDisk.DiskProperties.Name);
            return result;
        }

        public static void GetOccupiedSpace()
        {
            if (Disks.Count == 0)
            {
                Console.ErrorMessage("Open a virtual disk before using this command.");
                return;
            }

            var divisor = 1;
            while (CurrentDisk.DiskProperties.NumberOfUsedBlocks*CurrentDisk.DiskProperties.BlockSize/
                   ((int) Math.Pow(1024, divisor - 1)) > 1024)
            {
                divisor++;
            }
            Console.Message((CurrentDisk.DiskProperties.NumberOfUsedBlocks*
                            CurrentDisk.DiskProperties.BlockSize/(Math.Pow(1024, divisor - 1))).ToString("#.##") + IdToSize[divisor - 1] + " space is occupied on " +
                            CurrentDisk.DiskProperties.Name);
        }

        /// <summary>
        /// Prints the helptext into the console.
        /// </summary>
        public static void Help()
        {
            Console.Message(
                "Available Commands:\n" +
@"createdisk -s Size [-p SysPath, -n Name, -b BlockSize, -pw Password]
loaddisk SysPath [-pw Password]
unloaddisk Name
removedisk Name
listdisks [-p SysPath]

ls [-p Path, -f Files, -d Directories]
cd Path
mkdir Path
mk Path
remove Path
rename Path Name
move Path Path
copy Path Path
import SysPath Path
export Path SysPath

free
occ
help
defrag
exit"
                );
        }

        public static void Exit()
        {
            foreach (var vfsDisk in Disks.ToList())
            {
                UnloadDisk(vfsDisk.DiskProperties.Name);
            }
            Console.Message("Do you want to exist?");
        }

        /// <summary>
        /// Defragmentates the current disk
        /// </summary>
        public static void Defrag()
        {
            var lastUsedBlockAddress = 0;
            for (var i = 0; i < CurrentDisk.BitMap.Count; i++)
            {
                if (CurrentDisk.BitMap[i])
                {
                    lastUsedBlockAddress = i;
                }
            }

            var currentAddress = 0;
            while (lastUsedBlockAddress > currentAddress)
            {
                if (!CurrentDisk.BitMap[currentAddress])
                {
                    if (!CurrentDisk.Allocate(currentAddress))
                    {
                        currentAddress++;
                        continue;
                    }
                    CurrentDisk.Move(lastUsedBlockAddress, currentAddress);
                    for (int i = lastUsedBlockAddress; i > 0; i--)
                    {
                        if (CurrentDisk.BitMap[i])
                        {
                            lastUsedBlockAddress = i;
                            break;
                        }
                    }
                }
                currentAddress++;
            }
            CurrentDisk.Stream.SetLength((lastUsedBlockAddress + 1) * (long)CurrentDisk.DiskProperties.BlockSize);
        }
    }
}
