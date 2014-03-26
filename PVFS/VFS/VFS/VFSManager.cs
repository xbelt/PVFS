using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class VFSManager
    {
        private readonly static List<VfsDisk> _disks = new List<VfsDisk>();
        public static VfsDirectory workingDirectory;
        public static VfsDisk CurrentDisk;
        public static VfsConsole Console = new VfsConsole();
        private readonly static string[] idToSize = { "bytes", "kb", "mb", "gb", "tb" };

        #region Private Methods


        /// <summary>
        /// Returns the disk with the corresponding name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns a VfsDisk if it was found, otherwise null.</returns>
        private static VfsDisk getDisk(string name)
        {
            return _disks.FirstOrDefault(d => d.DiskProperties.Name.Equals(name));
        }

        /// <summary>
        /// Returns the corresponding VfsEntry
        /// </summary>
        /// <param name="path">An absolute path containing the disk name</param>
        /// <returns>Returns the entry if found, otherwise null.</returns>
        private static VfsEntry getEntry(string path)
        {
            //TODO: add support for relative paths
            VfsDirectory last;
            IEnumerable<string> remaining;
            return getEntry(path, out last, out remaining);
        }

        /// <summary>
        /// Returns the corresponding VfsEntry
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
            {
                var diskRoot = getDisk(path.Substring(1));
                last = diskRoot.root;
                //TODO: return null if disk null
                remaining = new List<string>();
                return last;
            }

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
        /// Returns the corresponding VfsEntry. 
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
        /// Returns the corresponding VfsEntry.
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
            for (int i = 1; i < names.Length - 1; i++)
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

        #endregion

        //----------------------Helper----------------------

        /// <summary>
        /// Returns the absolute path for the supplied path/identifier.
        /// For identifier: workingDirectory/ident
        /// Supports . and ..
        /// </summary>
        /// <param name="path">The path or identifier.</param>
        /// <returns>A valid absolute path.</returns>
        public static string getAbsolutePath(string path)
        {
            if (path == null)
                throw new ArgumentException("path");

            if (path == "." || path == "")
                return workingDirectory.GetAbsolutePath();
            else if (path == "..")
                return (workingDirectory.Parent ?? workingDirectory).GetAbsolutePath();
            else if (path.StartsWith("/"))
                return path;
            else
                return workingDirectory.GetAbsolutePath() + "/" + path;
        }


        //----------------------Disk----------------------

        public static void AddAndOpenDisk(VfsDisk disk)
        {
            _disks.Add(disk);
            CurrentDisk = disk;
            workingDirectory = disk.root;

            Console.Message("Opened disk " + disk.DiskProperties.Name + ".");
        }
        
        public static void UnloadDisk(string name)
        {
            var unmountedDisks = _disks.Where(x => x.DiskProperties.Name == name);
            foreach (var unmountedDisk in unmountedDisks)
            {
                unmountedDisk.getReader().Close();
                unmountedDisk.getWriter().Close();

                Console.Message("Closed disk " + unmountedDisk.DiskProperties.Name + ".");
            }
        }


        //----------------------Working Directory----------------------

        /// <summary>
        /// Changes the working directory to a new one by path.
        /// </summary>
        /// <param name="path">The path to the new working directory.</param>
        public static void ChangeWorkingDirectory(string path)
        {
            VfsEntry entry = getEntry(path);

            if (entry == null || !entry.IsDirectory)
            {
                Console.Error("This path does not exist.");
                return;
            }

            workingDirectory = (VfsDirectory) entry;
            CurrentDisk = workingDirectory.Disk;

            Console.Message("New working directory: " + path);
        }

        // copy
        public static void ChangeDirectoryByIdentifier(string name)
        {
            workingDirectory = workingDirectory.GetDirectory(name) ?? workingDirectory;
            Console.Message("New working directory: " + name);
        }

        public static void navigateUp()
        {
            if (workingDirectory.Parent != null)
                workingDirectory = workingDirectory.Parent;

            Console.Message("New working directory: " + workingDirectory.GetAbsolutePath());
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
            VfsEntry entry;
            if (path.IndexOf("/", 1) == -1)
            {
                entry = CurrentDisk.root;
            }
            else
            {
                entry = getEntry(path);
            }

            if (entry == null)
            {
                Console.Error("Directory not found.");
                return;
            }

            if (!entry.IsDirectory)
            {
                Console.Error("This is not a directory.");
                return;
            }

            VfsDirectory dir = (VfsDirectory) entry;

            if (dirs)
            {
                Console.Message(dir.GetDirectories().Aggregate("",(current, d)=>current + " " + d.Name).TrimEnd(' '));
            }
            if (files)
            {
                Console.Message(dir.GetFiles().Aggregate("", (current, d) => current + " " + d.Name).TrimEnd(' '), ConsoleColor.Blue);
            }
        }

        /// <summary>
        /// Creates directories such that the whole path given exists. If the path contains a file this does nothing.
        /// </summary>
        /// <param name="path">The path to create.</param>
        /// <param name="silent">Indicates whether there should be an output into the console if the operation was successful.</param>
        /// <returns>Returns true if the operation succeded, otherwise false.</returns>
        public static bool CreateDirectory(string path, bool silent)
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
                if (!silent)
                    Console.Message("Created the path " + path + ".");
                return true;
            }
            if (entry.IsDirectory)
            {
                Console.Message("This Directory already existed.");
                return true;
            }
            Console.Error("A file with the same name already existed.");
            return false;
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

            Console.Message("File successfully created.");
        }

        /// <summary>
        /// Moves a File or Directory to a new location. A directory is moved recursively.
        /// </summary>
        /// <param name="srcPath">The absolute path to the File/Directory that should be moved.</param>
        /// <param name="dstPath">The absolute path to the target Directory.</param>
        public static void Move(string srcPath, string dstPath)
        {
            //TODO: prevent from deleting root
            if (srcPath == null || dstPath == null)
                throw new ArgumentNullException("", "Argument null.");

            if (dstPath.StartsWith(srcPath)) // TODO: make a real recursive test
            {
                Console.Error("Can't move a directory into itself!");
                return;
            }
            if (!srcPath.StartsWith("/"))
            {
                srcPath = workingDirectory.GetAbsolutePath() + "/" + srcPath;
            }

            if (!dstPath.StartsWith("/"))
            {
                dstPath = workingDirectory.GetAbsolutePath() + "/" + dstPath;
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

            if (dst.GetEntry(src.Name) != null)
            {
                Console.Error("There is already a file with that name in the target directory.");
                return;
            }

            if (src.Disk != dst.Disk)
            {
                string tmp = getTempFilePath();

                Export(tmp, srcPath);
                Import(tmp, dstPath);
                Remove(srcPath);
                File.Delete(tmp);
            }
            else
            {
                src.Parent.RemoveElement(src);
                dst.AddElement(src);
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
            //TODO: prevent from deleting root
            if (src == null) throw new ArgumentNullException("src");
            if (newName == null) throw new ArgumentNullException("newName");

            VfsDirectory last;
            IEnumerable<string> remaining;
            VfsEntry entry = getEntry(src, out last, out remaining);

            if (newName.Length > VfsFile.MaxNameLength)
            {
                Console.Error("This name is too long.");
                return;
            }

            if (entry == null)
            {
                Console.Error("This file does not exist.");
                return;
            }

            if (last.GetEntry(newName) != null)
            {
                Console.Error("There already exists a file or directory with this name.");
                return;
            }

            ((VfsFile)entry).Rename(newName);

            Console.Message("Renamed file to " + newName + ".");
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

            if (!srcPath.StartsWith("/"))
            {
                srcPath = workingDirectory.GetAbsolutePath() + "/" + srcPath;
            }

            if (!dstPath.StartsWith("/"))
            {
                dstPath = workingDirectory.GetAbsolutePath() + "/" + dstPath;
            }

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
                if (!CreateDirectory(dstPath, true))
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

            Console.Message("Copied " + srcPath + " to " + dstPath + ".");
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

        /// <summary>
        /// Imports a File from the host Filesystem to a directory inside the virtual Filesystem. (are we supporting importing whole directories too?)
        /// Overwrites already existing files with the same name.
        /// Creates the target directory if it doesn't exist. Introduces grandpa Joe.
        /// 
        /// Throws an exception if you try to import the virtual disk itself!
        /// </summary>
        /// <param name="src">The absolute path to the File (or Directory) that should be imported (Host file system).</param>
        /// <param name ="disk">The disk in which the file is imported</param>
        /// <param name="parent">the parent directory of the destination we're importing</param>
        private static VfsFile ImportFile(string src, VfsDisk disk, VfsDirectory parent) //TODO: 'parent' is a a bit misleading to me. Might rename it
        {
            //Check if file or directory exists at src TODO: probably redundant because of caller
            if (!File.Exists(src))
            {
                Console.Message("invalid path: file does not exist at " + src);
                return null;
            }

            //Get FileLength
            var fileInfo = new FileInfo(src);
            //Console.Message("Trying to import:" + fileInfo.Name);
            var fileLength = Convert.ToInt32(fileInfo.Length);  //TODO: Might lose precision...
            
            //Name of file to write
            var fileName = fileInfo.Name;

            //check if there's already a file with that name
            if (parent.GetFiles().SkipWhile(e => !e.Name.Equals(fileName)).Count() != 0)
            {
                //throw new ArgumentException("this directory already has a file with the name " + fileName);
                var answer = Console.Query("There's already a file with the name:" + fileName + ". Do you want to overwrite it?",
                    "Ok", "Cancel");
                if (answer == 1)
                { //Cancel
                    Console.Message("Canceled import operation");
                    return null; //Will make the 'Import' function return
                }
                //Remove the already existing file
                var toRemove = parent.GetFile(fileName);
                Remove(toRemove.GetAbsolutePath());
            }


            //Create entry in which to write the file
            var importEntry = (VfsFile) EntryFactory.createFile(disk, fileName, fileLength, parent);

            //Start actual import
            var reader = new BinaryReader(fileInfo.OpenRead());
            importEntry.Write(reader);
            parent.AddElement(importEntry);
            
            //Dispose resources and close reader
            reader.Dispose();
            reader.Close();
            
            return importEntry;
        }
        public static void Import(string src, string dst)
        {
            //TODO: check file names to be compatible with naming conventions
            //TODO: prevent import of currently opened disk
            //TODO: add compression and encryption
            if (src == null) throw new ArgumentNullException("src");
            if (dst == null) throw new ArgumentNullException("dst");

            //Get disk and parent directory:
            VfsDirectory dstDirectory;
            IEnumerable<string> remaining;
            getEntry(dst, out dstDirectory, out remaining);
            if (dstDirectory == null)
                throw new Exception("the entry you want to access was null.");

            if (dstDirectory.Disk == null)
            {
                Console.Message("Your destination path was invalid: " + dst);
                Console.Message("Aborting import operation.");
                return;
            }
            var disk = dstDirectory.Disk;

            //Check if dstDirectory is correct
            if (!dstDirectory.Name.Equals(dst.Substring(dst.LastIndexOf('\\') + 1, dst.Length - 1))) //TODO: check substring
            {
                var remainingPath = remaining.Aggregate("", (current, item) => current + (item + '/'));
                int answer = Console.Query("Path leads to unexisting folders. Do you want to create them? " + remainingPath, "Ok", "Cancel");
                if (answer == 0)
                {
                    //Create the path
                    CreateDirectory(dst, false);
                }
                else
                {
                    Console.Message("Canceled operation.");
                    return;
                }
            }

            //Check type of src
            if (Directory.Exists(src))
            {
                //Get files and subfolders
                var filePaths = Directory.GetFiles(src);
                var dirPaths  = Directory.GetDirectories(src);

                //Import files
                if (filePaths.Length != 0) 
                {
                    foreach (string path in filePaths) 
                    {
                        ImportFile(path, disk, dstDirectory);
                    } 
                }
                
                //If there are no subfolders we're done
                if (dirPaths.Length == 0) return;
                
                //Import SubDirectories
                foreach (var dirPath in dirPaths)
                {
                    //Will be parent for files in subfolders of src
                    var newParDir = ImportDirectory(dirPath, disk, dstDirectory);
                    
                    if (newParDir == null) //Happens if import has been canceled
                        return;
                    
                    if (!newParDir.IsDirectory)
                        throw new Exception("imported a file as a directory - weird error");
                    //Recursive call to import files in subfolder
                    Import(dirPath, dst + '/' + newParDir.Name);
                }
            } 
            else if (File.Exists(src))
            {
                ImportFile(src, disk, dstDirectory);
            }
            else
            {
                throw new ArgumentException("the path:" + src + " does not lead to a file or directory.");
            }
        }

        private static VfsDirectory ImportDirectory(string src, VfsDisk disk, VfsDirectory parent)
        {
            //Check if directory exists at src TODO: probably redundant because of caller
            if (!Directory.Exists(src))
            {
                Console.Message("invalid path: directory does not exist at " + src);
                return null;
            }

            //Get FileLength
            var dirInfo = new DirectoryInfo(src);

            //Name of file to write
            var dirName = dirInfo.Name;

            //check if there's already a directory with that name
            if (parent.GetFiles().SkipWhile(e => !e.Name.Equals(dirName)).Count() != 0)
            {
                var answer = Console.Query("There's already a directory with the name:" + dirName + ". Do you want to overwrite it and it's content?",
                    "Ok", "Cancel");
                if (answer == 1)
                { //Cancel
                    Console.Message("Canceled import operation");
                    return null; //Will make the 'Import' function return
                }
                //Remove the already existing file
                var toRemove = parent.GetDirectory(dirName);
                Remove(toRemove.GetAbsolutePath());
            }


            //Create directory
            CreateDirectory(parent.GetAbsolutePath() + '/' + dirName, false);

            return parent.GetDirectory(dirName);            
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
            if (!Directory.Exists(dst))
            {
                Console.Message("Destination does not lead to a directory: " + dst);
                return;
            }

            //Check if src is rootdirectory
            if (src.LastIndexOf('/') == 0)
            {
                var disk = getDisk(src.Substring(1, src.Length -1));
                if (disk == null) { Console.Message("disk is null");}
                ExportDirectory(dst, disk.root, true);
                return;
            }
            //get entry to export
            var toExport = getEntry(src);
            if (toExport == null)
            {
                Console.Message("Invalid path to source entry: " + src);
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
                throw new NullReferenceException("The file to export is null.");
            var entryName = toExport.Name;
            if (entryName == null) //TODO: probably useless check
               throw new NullReferenceException("Name of file is null.");
            
            //Create the path to destination (non existent folders are automatically created)
            Directory.CreateDirectory(dst);
            
            //Get path including fileName
            //TODO: What about those extensions?
            var completePath = Path.Combine(dst, entryName);

            //Check if file with same name already exists at that location
            if (File.Exists(completePath))
            {
                Console.Message("There's already a file with the same name in the host file system");
                return;
            }

            //TODO: might be more efficient to pass these 
            //as argument or have them as fields instead of making
            //them for each file
            //Start actual export
            var fs = File.Create(completePath);
            var writer = new BinaryWriter(fs);
            toExport.Read(writer);
            
            //Close and dispose resources
            writer.Dispose();
            fs.Dispose();
            writer.Close();
            fs.Close();
        }

        private static void ExportDirectory(string dst, VfsDirectory toExport, bool isFirstRecursion) 
        {
            var filesInDir = toExport.GetFiles().ToList();
            var subDirs = toExport.GetDirectories().ToList();
            var path = dst;
            //Export directory
            //TODO: check for correctness
            if (isFirstRecursion) 
                path += '/' + toExport.Name;

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
                path += '/' + subDir.Name;
                Console.Message(path);
                ExportDirectory(path, subDir, false);
            }
        }

        /// <summary>
        /// Deletes a file or directory from the disk.
        /// </summary>
        /// <param name="path">THe path to the file/directory.</param>
        public static void Remove(string path)
        {
            //TODO: prevent from deleting root
            var entry = getEntry(path) as VfsFile;

            if (entry == null)
            {
                Console.Error("This file does not exist.");
                return;
            }

            if (entry.IsDirectory)
            {
                var files = ((VfsDirectory)entry).GetFiles();
                foreach (var file in files)
                {
                    file.Free();
                }
                var directories = ((VfsDirectory)entry).GetDirectories();
                foreach (var directory in directories)
                {
                    Remove(directory.GetAbsolutePath());
                }
            }
            entry.Parent.RemoveElement(entry);
            entry.Free();

            Console.Message("Successfully removed " + entry.Name + ".");
        }

        // copy
        public static void RemoveByIdentifier(string ident)
        {
            //TODO: prevent from deleting root
            var entry = getEntry(CurrentDisk, workingDirectory.GetAbsolutePath() + "/" + ident) as VfsFile;

            if (entry == null)
            {
                Console.Error("This file does not exist.");
                return;
            }

            if (entry.IsDirectory)
            {
                var files = ((VfsDirectory)entry).GetFiles();
                foreach (var file in files)
                {
                    file.Free();
                }
                var directories = ((VfsDirectory)entry).GetDirectories();
                foreach (var directory in directories)
                {
                    Remove(directory.GetAbsolutePath());
                }
            }
            entry.Parent.RemoveElement(entry);
            entry.Free();

            Console.Message("Successfully removed " + entry.Name + ".");
        }

        //----------------------Disk Properties----------------------

        public static void GetFreeSpace()
        {
            var divisor = 1;
            while ((CurrentDisk.DiskProperties.NumberOfBlocks - CurrentDisk.DiskProperties.NumberOfUsedBlocks)*
                   CurrentDisk.DiskProperties.BlockSize/((int) Math.Pow(1024, divisor - 1)) > 1024)
            {
                divisor++;
            }
            Console.Message(((CurrentDisk.DiskProperties.NumberOfBlocks - CurrentDisk.DiskProperties.NumberOfUsedBlocks)*
                            CurrentDisk.DiskProperties.BlockSize/(Math.Pow(1024, divisor - 1))).ToString("0.##") + idToSize[divisor - 1] + " free space available on " +
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
            Console.Message("Bye");
        }

        public static void Defrag()
        {
            //TODO: this is not optimal in case: 1110000001 -> 1110000010
            var numberOfUnusedBlocks = 0;
            var lastUsedBlockAddress = 0;
            for (var i = 0; i < CurrentDisk.bitMap.Count; i++)
            {
                if (CurrentDisk.bitMap[i])
                {
                    lastUsedBlockAddress = i;
                }
            }
            for (var i = 0; i < lastUsedBlockAddress; i++)
            {
                if (!CurrentDisk.bitMap[i])
                {
                    numberOfUnusedBlocks++;
                }
            }
            int[] addresses;
            CurrentDisk.Allocate(out addresses, numberOfUnusedBlocks);

            numberOfUnusedBlocks--;

            for (var i = lastUsedBlockAddress; i >= 0; i--)
            {
                if (numberOfUnusedBlocks < 0)
                {
                    break;
                }
                if (CurrentDisk.bitMap[i])
                {
                    CurrentDisk.Move(i, addresses[numberOfUnusedBlocks--]);
                }
            }

            for (var i = 0; i < CurrentDisk.bitMap.Count; i++)
            {
                if (CurrentDisk.bitMap[i])
                {
                    lastUsedBlockAddress = i;
                }
            }

            CurrentDisk.Stream.SetLength((lastUsedBlockAddress + 1)*(long) CurrentDisk.DiskProperties.BlockSize);
        }
    }
}
