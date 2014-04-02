using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VFS.VFS.Models;

namespace VFS.VFS
{
    public class VFSManager
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
        public static VfsEntry getEntry(string path)
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
        public static VfsEntry getEntry(string path, out VfsDirectory last, out IEnumerable<string> remaining)
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
        public static VfsEntry getEntry(VfsDisk disk, string path)
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
        public static VfsEntry getEntry(VfsDisk disk, string path, out VfsDirectory last, out IEnumerable<string> remaining)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            VfsDirectory current = disk.root;
            string[] names = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 0)
                throw new ArgumentException("Path not valid");
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
                throw new ArgumentNullException("path");

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

        /// <summary>
        /// Lists the currently loaded disks.
        /// </summary>
        public static void ListDisks()
        {
            Console.Message(_disks.Aggregate("", (curr, d) => curr + " " + d.DiskProperties.Name));
        }

        public static void AddAndOpenDisk(VfsDisk disk)
        {
            _disks.Add(disk);
            CurrentDisk = disk;
            workingDirectory = disk.root;

            Console.Message("Opened disk " + disk.DiskProperties.Name + ".");
        }
        
        public static void UnloadDisk(string name)
        {
            var unmountedDisks = _disks.Where(x => x.DiskProperties.Name == name).ToList();
            foreach (var unmountedDisk in unmountedDisks)
            {
                unmountedDisk.getReader().Close();
                unmountedDisk.getWriter().Close();

                Console.Message("Closed disk " + unmountedDisk.DiskProperties.Name + ".");
                _disks.Remove(unmountedDisk);
            }
        }

        public static void CreateDisk(string path, string name, double size, int blockSize, string pw)
        {
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            if (!Directory.Exists(path))
            {
                Console.Error("Directory does not exist.");
                return;
            }
            if (File.Exists(path + name + ".vdi"))
            {
                Console.Error("This disk already exists");
                return;
            }
            if (blockSize%4 != 0 || blockSize <= FileOffset.Header)
            {
                Console.Error("Blocksize must be a multiple of 4 and larger than 128!");
                return;
            }
            if (size < blockSize || size/blockSize < 5)
            {
                Console.Error("The disk must at least hold 5 blocks.");
                return;
            }
            var disk = DiskFactory.Create(new DiskInfo(path, name, size, blockSize), pw);
            AddAndOpenDisk(disk);
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
            ChangeWorkingDirectory(workingDirectory.GetAbsolutePath() + "/" + name);
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
        public static VfsFile CreateFile(string path)
        {
            VfsDirectory last;
            IEnumerable<string> remaining;
            VfsEntry entry = getEntry(path, out last, out remaining);

            if (entry != null)
            {
                Console.Error("There already existed a file or directory with the same path.");
                return null;
            }

            if (last == null)
            {
                Console.Error("The disk was not found.");
                return null;
            }

            List<string> fileNames =  remaining.ToList();
            if (fileNames.Count > 1)
            {
                Console.Error("Target directory was not found.");
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

    /*    private static string MakeStringValid(string str) 
        {
            var corrected = str.Where(c => Char.IsLetterOrDigit(c) || c == '-' || c == '/' || c == '_' || c == '.').ToString();


            while (corrected.Contains("..") || corrected.Contains("\\") || corrected.Contains("//"))
            {
                corrected = corrected.Replace("..", ".");
                corrected = corrected.Replace('\\', '/');
                corrected = corrected.Replace("//", "/");
            }
            Console.Message(corrected);
            return corrected;
        }

        private static string ConvertToValidString(string arg)
        {
            var hasBeenModified = false;
            var isValid = false;
            return ConvertToValidString(arg, out hasBeenModified, out isValid);
        }
        public static string ConvertToValidString(string str, out bool hasBeenConverted, out bool isValid ) 
        {
            if (str == null) throw new ArgumentNullException("str");

            var corrected = MakeStringValid(str);
            Console.Message(corrected);
            //Check if we did modify anything
            isValid = corrected.Equals(str);

            if (!isValid)
            {
                Console.Message("Your path was not valid. The characters have to be in:");
                Console.Message("[a-z , A-Z , 0-9 , - , _ , / , . ] where no / or . can follow a / or . respectively.");
                Console.Message("Do you agree with this new suggestion:\n" + corrected + " ?" );
                var answer = Console.Query("Write 'Ok' or 'Cancel'.", "Ok", "Cancel");
                if (answer == 0)
                {
                    hasBeenConverted = true;
                    return corrected;
                } 
            }
            var report = isValid ? "String was valid" : "String was not valid";
            Console.Message("String has not been converted because " + report);
            hasBeenConverted = false;
            return str; //TODO: maybe better to make this null so that no invalid strings can be used.
        } */

        private static string Compress(FileInfo fileToCompress) {
            String returnPath;
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if (( File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden ) == FileAttributes.Hidden & fileToCompress.Extension != ".gz") 
                    return fileToCompress.FullName; //Was an additional condition: 

             //   var fileToCompressName = fileToCompress.FullName.Substring(0,fileToCompress.FullName.LastIndexOf('.')) + ".gz";
                using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                {
                    returnPath = fileToCompress.FullName + ".gz";
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                        Console.Message("Compressed " + fileToCompress.Name + " from " + fileToCompress.Length.ToString() + " to " + compressedFileStream.Length.ToString() + " bytes.");
                    }
                }
            }
            return returnPath;
        }

        public static void Decompress(FileInfo fileToDecompress, string type)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);
                
                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.Message("Decompressed: " + fileToDecompress.Name);
                    }
                }
            }
        }
        /// <summary>
        /// Imports a File from the host Filesystem to a directory inside the virtual Filesystem. (are we supporting importing whole directories too?)
        /// Overwrites already existing files with the same name.
        /// Creates the target directory if it doesn't exist. Introduces grandpa Joe.
        /// 
        /// Throws an exception if you try to import the virtual disk itself! TODO
        /// </summary>
        /// <param name="src">The absolute path to the File (or Directory) that should be imported (Host file system).</param>
        /// <param name="dst">The absolute path to the directory where we import.</param>
        public static void Import(string src, string dst) 
        {
            //TODO: correct prevention of import of currently opened disk
            //TODO: add compression and encryption
            if (src == null) throw new ArgumentNullException("src");
            if (dst == null) throw new ArgumentNullException("dst");

            //Check if destination exists, create it on request and get the entry:
            //var dstDir = (VfsDirectory) GetEntryIfPathValid(dst);
            var dstDir = (VfsDirectory) getEntry(dst);
            //Check if User aborted operation
            if (dstDir == null) return;

            //Check if destination was a file (we can't import into a file).
            if (!dstDir.IsDirectory)
            {
                Console.Message("Your destination leads to a file. Aborting import operation.");
                return;
            }

            //Check if source is valid: TODO: also check file and dir names
            if (File.Exists(src))
            {
                //Check that it's not the disk we've opened
                //TODO: this is not entirely correct but I haven't found a better method...
                var lastName = src.Substring(src.LastIndexOf('\\') + 1);
                if ((dstDir.Disk.root.Name + ".vdi").Equals(lastName))
                {
                    Console.Message("You're not allowed to import the currently opened disk. Aborted import.");
                    return;
                }
                ImportFile(src, dstDir);
            }
            else if (Directory.Exists(src))
            {
                ImportDirectory(src, dstDir);
            }
            else
            {
                Console.Message("Your source path does not lead to a valid file or directory. Aborting import operation.");
            }
        }

        private static void ImportFile(string src, VfsDirectory dstDir) 
        {

            //Get FileInfo
            var toCompress = new FileInfo(src);
            
            //Compress the file before importing
            var compressedSrc = Compress(toCompress);

            //get the compressed file
            var fileInfo = new FileInfo(compressedSrc);
            var fileName = fileInfo.Name;

        //    fileName = MakeStringValid(fileName); //TODO: is it okay to just change it without asking?
            Console.Message("Importing " + fileName + " in " + dstDir.GetAbsolutePath());

            //Check for duplicates
            var fileWithSamename = dstDir.GetFile(fileName);
            if (fileWithSamename != null)
            {
                Console.Message("There is already a file with the name: " + fileName + ".");
                var answer = Console.Query("Do you want to overwrite it? Write 'Ok' or 'Cancel'. ", "Ok", "Cancel");
                if (answer == 1)
                {
                    Console.Message("File has not been overwritten.");
                    return;
                }
                //Delete file to create a new one.
                Console.Message("Removing " + fileWithSamename.Name);
                Remove(fileWithSamename.GetAbsolutePath()); //Works because of duplicate name.
            }

            //Get fileLength
            var fileLength = Convert.ToInt32(fileInfo.Length);

            //Create entry in which to write the file
            var importEntry = EntryFactory.createFile(dstDir.Disk, fileName, fileLength, dstDir);

            //Start actual import
            var reader = new BinaryReader(fileInfo.OpenRead());
            importEntry.Write(reader);

            //Dispose resources and close reader
            reader.Dispose();
            reader.Close();

            //Delete the compressed file in host system (what kind of compression would it be to have the same file twice? :P)
            File.Delete(compressedSrc);
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
                Remove(dirWithSameName.GetAbsolutePath()); //Work because of duplicate name
            }

            //Create directory
            Console.Message("Importing " + dirName + " in " + dstDir.GetAbsolutePath());
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
        private static VfsEntry GetEntryIfPathValid(string dst)
        {
            if (dst == null) throw new ArgumentNullException("dst");

            //Check if path is valid and correct it if not.
            bool dstHasBeenChanged;
            bool isValid;
        /*    var correctedDst = ConvertToValidString(dst, out dstHasBeenChanged, out isValid);
            VfsEntry dstDir;
            if (isValid)
            {
                dstDir = getEntry(dst); //Could also be correctedDst, doesn't matter in this case.
            }
            else
            {
                if (dstHasBeenChanged)
                {
                    dstDir = getEntry(correctedDst);
                }
                else
                {
                    Console.Message("You're not allowed to use invalid names. Aborted operation.");
                    return null;
                }
            } */

            var dstDir = getEntry(dst); 
            if (dstDir != null)
            {
                Console.Message(dstDir.Name + "is the destination Directory.");
                if (dstDir.IsDirectory)
                    return dstDir;
                //Destination is not a directory
                Console.Message("Your destination does not lead to a directory:\n" + dst);
                Console.Message("Please enter a valid path. Operation is aborted.");
                return null;
            }
            //Destination was invalid
            Console.Message("Invalid destination. Do you want to create the path: " + dst);
            var answer = Console.Query("Write 'Ok' or 'Cancel'.", "Ok", "Cancel");
            if (answer == 0) //Ok
            {
                CreateDirectory(dst, true);
                Console.Message("Created the following path:" + ((VfsFile) getEntry(dst)).GetAbsolutePath());
                //Get entry again so it's not null
                return getEntry(dst);
            }
            Console.Message("Aborted operation.");
            return null;
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

            //If I don't have to decompress we're done
            if (!toExport.Type.Equals("gz")) return;
           
            //Decompress file
            var toDecompress = new FileInfo(completePath);
            var typeExtension = toExport.Type;
            Decompress(toDecompress, typeExtension);

            //Delete compressed file
            File.Delete(toDecompress.FullName);
          
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
                ExportDirectory(path, subDir, false);
            }
        }

        /// <summary>
        /// Deletes a file or directory from the disk.
        /// </summary>
        /// <param name="path">THe path to the file/directory.</param>
        public static void Remove(string path)
        {
            //TODO: prevent from deleting root --> Done, see if statement
            var entry = getEntry(path) as VfsFile;

            if (entry == null)
            {
                Console.Error("This file does not exist.");
                return;
            }

            if (entry.IsDirectory)
            {
                if (entry == entry.Disk.root)
                {
                    Console.Message("You're not allowed to delete the root directory.");
                    return;
                }
                var files = ((VfsDirectory)entry).GetFiles();
                foreach (var file in files)
                {
                    file.Free();
                }
                var directories = ((VfsDirectory)entry).GetDirectories();
                foreach (var directory in directories)
                {
                    Console.Message("Removing " + directory.GetAbsolutePath());
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
            //TODO: prevent from deleting root --> Done, see if-statement (overthink this again...)
            var entry = getEntry(CurrentDisk, workingDirectory.GetAbsolutePath() + "/" + ident) as VfsFile;

            if (entry == null)
            {
                Console.Error("This file does not exist.");
                return;
            }

            if (entry.IsDirectory)
            {
                if (entry == entry.Disk.root) 
                {
                    //TODO: Maybe this is not such a good idea. Now Remove of disks does not work anymore :/
                    //TODO: Maybe it's best to make a query here
                    Console.Message("You're not allowed to delete the root directory.");
                    return;
                }
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
