using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFS.VFS.Extensions;

namespace VFS.VFS.Models
{
    /*
 * NextBlock 4
 * StartBlock 4
 * NrOfChildren 4
 * NoBlocks 4
 * Directory 1
 * NameSize 1
 * Name 110
 * Children x * 4
 */

    //TODO: GetFile-Functions could use NrOfChildren instead of Elements.Count
    public class VfsDirectory : VfsFile
    {
        public VfsDirectory(VfsDisk disk, int address, string name, VfsDirectory parent, int noEntries, int noBlocks, int nextBlock)
            : base(disk, address, name, parent, noEntries, noBlocks, nextBlock)
        {
            IsDirectory = true;
            // careful filesize is == noEntrys in this class!
        }

        /// <summary>
        /// Loads the directory, fills Inodes and Elements. Should only be called in getEntries();
        /// </summary>
        private void Load()
        {
            var reader = Disk.getReader();
            Inodes = new List<Block> { new Block(Address, Address, null) };
            var elementAddresses = new List<int>();
            int head = HeaderSize, doneEntriesInCurrentBlock = 0, totalEntries = 0;
            var nextBlock = NextBlock;

            reader.Seek(Disk, Address, head);
            while (totalEntries < FileSize)
            {
                elementAddresses.Add(reader.ReadInt32());
                totalEntries++;
                doneEntriesInCurrentBlock++;

                // Current Block exhausted?
                if (doneEntriesInCurrentBlock >= (Disk.BlockSize - head) / 4 && totalEntries != FileSize)
                {
                    var next = new Block(nextBlock, Address, null);
                    Inodes.Last().NextBlock = next;
                    Inodes.Add(next);

                    reader.Seek(Disk, nextBlock);
                    nextBlock = reader.ReadInt32();
                    if (reader.ReadInt32() != Address)
                        throw new IOException("The startBlock Address of block " + Inodes.Last().Address + " was inconsistent.");

                    doneEntriesInCurrentBlock = 0;
                    head = SmallHeaderSize;
                }
            }

            if (nextBlock != 0)
                throw new IOException("The nextBlock Address of block " + Inodes.Last().Address + " is not 0 (it's the last block).");

            // TODO F: see remark at Elements definition
            Elements = elementAddresses.Select(address => EntryFactory.OpenEntry(Disk, address, this)).ToList();

            IsLoaded = true;
        }



        // TODO F: this should be a VfsEntry list, because it can contain both, VfsFiles and VfsDirectories, otherwise it's missleading. Maybe take VfsFile.Name, etc into VfsEntry?
        public List<VfsEntry> Elements { get; set; }

        public VfsDirectory GetSubDirectory(string name)
        {
            try
            {
                return Elements.Single(el => el.IsDirectory) as VfsDirectory;
            }
            catch (Exception e)
            {
                throw new InvalidDirectoryException("directory " + name + " not found");
            }
        }

        public IEnumerable<VfsDirectory> GetSubDirectories()
        {
            return Elements.Where(el => el.IsDirectory) as List<VfsDirectory>;
        }

        public IEnumerable<VfsFile> GetFiles()
        {
            return Elements.Where(element => !element.IsDirectory) as List<VfsFile>;
        }


        public VfsFile GetFileIgnoringSubDirectories(string name)
        {
            if (Elements.Count <= 0)
            {
                throw new Exception("Elements.Count was < 1");
            }
            if (name == null)
            {
                throw new Exception("Argument 'name' is null.");
            }

            return Elements.Where(element => !element.IsDirectory).FirstOrDefault(element => element.Name.Equals(name)) as VfsFile;
        }

        public VfsFile GetFileCheckingSubDirectories(string name)
        {
            VfsFile result;
            if (name == null)
                throw new Exception("argument 'name' was null");
            if (Elements.Count <= 0)
                throw new Exception("Elements.Count was < 1");//TODO F: This is no good for a recursive method!

            foreach (VfsFile element in Elements)
            {
                if (element.IsDirectory)
                {
                    var dir = (VfsDirectory)element;
                    result = dir.GetFileCheckingSubDirectories(name);
                    if (result != null)
                        return result;
                }
                else
                {
                    if (element.Name.Equals(name))
                    {
                        return element;
                    }
                }
            }
            throw new Exception("File " + name + " was not found");
        }

        //TODO: might throw exception
        public void AddElement(VfsFile element)
        {
            if (element != null)
            {
                if (Elements != null)
                {
                    Elements.Add(element);
                }
                else
                {
                    throw new Exception("Elements was null");
                }
            }
            else
            {
                throw new Exception("argument 'element' was null");
            }
        }

        //TODO: might throw excpetion
        public void RemoveElement(VfsFile element)
        {
            if (element != null)
            {
                if (Elements != null)
                {
                    Elements.Remove(element);
                }
                else
                {
                    throw new Exception("Elements was null");
                }
            }
            else
            {
                throw new Exception("argument 'element' was null");
            }
        }
    }

    public class InvalidDirectoryException : Exception
    {
        public InvalidDirectoryException(string s)
        {
            Console.WriteLine(s);
        }
    }
}
