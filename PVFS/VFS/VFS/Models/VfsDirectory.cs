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

    public class VfsDirectory : VfsFile
    {
        public VfsDirectory(VfsDisk disk, int address, string name, VfsDirectory parent, int noEntries, int noBlocks, int nextBlock)
            : base(disk, address, name, parent, noEntries, noBlocks, nextBlock)
        {
            this.IsDirectory = true;
        }

        /// <summary>
        /// Do not use, for Load method only. Use GetEntries() instead (this makes sure it's loaded).
        /// </summary>
        private List<VfsEntry> elements;
        private int noEntries
        {
            get { return this.FileSize; }
            set { this.FileSize = value; }
        }

        /// <summary>
        /// Loads the directory, fills Inodes and Elements. Should only be called in getEntries();
        /// </summary>
        private void Load()
        {
            if (this.noEntries == 0)
            {
                this.Inodes = new List<Block> { new Block(this.Address, this.Address, null) };
                this.elements = new List<VfsEntry>();
                this.IsLoaded = true;
                return;
            }


            BinaryReader reader = Disk.getReader();
            this.Inodes = new List<Block> { new Block(this.Address, this.Address, null) };
            List<int> elementAddresses = new List<int>();
            int head = HeaderSize, doneEntriesInCurrentBlock = 0, totalEntries = 0;
            int nextBlock = this.NextBlock;

            reader.Seek(Disk, this.Address, head);
            while (totalEntries < this.noEntries)
            {
                elementAddresses.Add(reader.ReadInt32());
                totalEntries++;
                doneEntriesInCurrentBlock++;

                // Current Block exhausted?
                if (doneEntriesInCurrentBlock >= (Disk.BlockSize - head) / 4 && totalEntries != this.noEntries)
                {
                    Block next = new Block(nextBlock, this.Address, null);
                    this.Inodes.Last().NextBlock = next;
                    this.Inodes.Add(next);

                    reader.Seek(Disk, nextBlock);
                    nextBlock = reader.ReadInt32();
                    if (reader.ReadInt32() != this.Address)
                        throw new IOException("The startBlock Address of block " + this.Inodes.Last().Address + " was inconsistent.");

                    doneEntriesInCurrentBlock = 0;
                    head = SmallHeaderSize;
                }
            }

            if (nextBlock != 0)
                throw new IOException("The nextBlock Address of block " + this.Inodes.Last().Address + " is not 0 (it's the last block).");

            elements = elementAddresses.Select(address => EntryFactory.OpenEntry(Disk, address, this)).ToList();

            this.IsLoaded = true;
        }

        /// <summary>
        /// Returns the entries contained in this directory and loads it if it was unloaded before.
        /// </summary>
        /// <returns>Returns the entries contained in this directory.</returns>
        public List<VfsEntry> GetEntries()
        {
            if (!this.IsLoaded)
                this.Load();

            return this.elements;
        }
        /// <summary>
        /// Looks for an entry with a given name.
        /// </summary>
        /// <returns>VfsEntry if one existed, otherwise null.</returns>
        public VfsEntry GetEntry(string name)
        {
            return this.GetEntries().FirstOrDefault(entry => entry.Name == name);
        }

        /// <summary>
        /// Stores the directory to disk.
        /// </summary>
        private void Store()
        {
            //TODO: L store also new name in case of rename and other properties
            if (noEntries == GetEntries().Count)
            {
                throw new Exception("Why did you call Store when nothing has changed?");
            }
            var writer = Disk.getWriter();
            var reader = Disk.getReader();
            int doneEntriesInCurrentBlock = 0, totalEntries = 0, blockNumber = 0, currentBlockAddress = Address;
            writer.Seek(Disk, Address, HeaderSize);
            FileSize = elements.Count;
            writer.Seek(Disk, Address, 8);
            writer.Write(FileSize);
            while (totalEntries < FileSize)
            {
                writer.Write(elements[totalEntries].Address);
                totalEntries++;
                doneEntriesInCurrentBlock++;

                // Current Block exhausted?
                if (doneEntriesInCurrentBlock >= (Disk.BlockSize - HeaderSize) / 4 && totalEntries != FileSize)
                {
                    blockNumber++;
                    if (blockNumber > NoBlocks)
                    {
                        int next;
                        Disk.allocate(out next);
                        writer.Seek(Disk, currentBlockAddress);
                        writer.Write(next);
                        writer.Seek(Disk, Address, 12);
                        writer.Write(++NoBlocks);
                        writer.Seek(Disk, next, SmallHeaderSize);
                    }
                    else
                    {
                        reader.Seek(Disk, currentBlockAddress);
                        currentBlockAddress = reader.ReadInt32();
                    }

                    doneEntriesInCurrentBlock = 0;
                }
            }
        }

        public IEnumerable<VfsDirectory> GetDirectories()
        {
            return this.GetEntries().Where(el => el.IsDirectory).Cast<VfsDirectory>().ToList();
        }
        public VfsDirectory GetDirectory(string name)
        {
            VfsEntry e = this.GetEntries().FirstOrDefault(entry => entry.Name == name);
            if (e != null && e.IsDirectory)
                return (VfsDirectory)e;
            else
                return null;
        }
        public IEnumerable<VfsFile> GetFiles()
        {
            return this.GetEntries().Where(el => !el.IsDirectory).Cast<VfsFile>().ToList();
        }
        public VfsFile GetFile(string name)
        {
            VfsEntry e = this.GetEntries().FirstOrDefault(entry => entry.Name == name);
            if (e != null && !e.IsDirectory)
                return (VfsFile)e;
            else
                return null;
        }

        /// <summary>
        /// Recursively looks for a name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VfsFile GetFileCheckingSubDirectories(string name)
        {
            VfsFile result;
            if (name == null)
                throw new Exception("argument 'name' was null");
            if (GetEntries().Count <= 0)
                throw new Exception("Elements.Count was < 1");//TODO F: This is no good for a recursive method!

            foreach (VfsFile element in elements)
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

        /// <summary>
        /// Adds an element to this directory. If it was already contained this does nothing.
        /// Throws an exception if there is not enough space on the disk to add this element.
        /// </summary>
        /// <param name="element">The element to add</param>
        /// <returns>Returns True if the element was sucessfully added to this directory, False if it was already contained in the directory.</returns>
        public bool AddElement(VfsFile element)
        {
            if (element == null)
                throw new ArgumentException("Argument 'element' was null.");

            if (!this.IsLoaded)
                this.Load();

            if (this.elements.Contains(element))
                return false;

            BinaryWriter writer = this.Disk.getWriter();

            if (GetNoBlocks(this.Disk, 4 * this.noEntries + 4) > this.NoBlocks)
            {// add a new block
                int address;
                if (!Disk.allocate(out address))
                    throw new ArgumentException("There is not enough space on this disk to add a new File to this directory!");
                Block block = new Block(address, this.Address, null), last = this.Inodes.Last();
                last.NextBlock = block;
                this.Inodes.Add(block);

                writer.Seek(this.Disk, last.Address);
                writer.Write(address);
                writer.Seek(this.Disk, address);
                writer.Write(0);
                writer.Write(this.Address);
            }
            else
            {
                // seek to end of used content
                // TODO: Please check really hard for index errors, there are probably some in here.
                int head = HeaderSize, noSubs, pos = this.noEntries;
                Block current = this.Inodes.First();
                while ((noSubs = (this.Disk.BlockSize - head) / 4) <= pos)// find block
                {
                    current = current.NextBlock;
                    pos -= noSubs;
                    head = SmallHeaderSize;
                }
                writer.Seek(this.Disk, current.Address, head + pos * 4);
            }

            writer.Write(element.Address);

            this.elements.Add(element);
            this.noEntries = this.elements.Count;
            writer.Seek(this.Disk, this.Address, 8);// Update noEntries
            writer.Write(this.noEntries);
            if (this.Inodes.Count != this.NoBlocks) // Update noBlocks
            {
                this.NoBlocks = this.Inodes.Count;
                writer.Write(this.NoBlocks);
            }
            return true;
        }

        /// <summary>
        /// Removes an element from this directory. If it's not in this directory this returns False.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        /// <returns>Returns True iff the element was in the directory, otherwise False.</returns>
        public bool RemoveElement(VfsFile element)
        {
            if (element == null)
                throw new ArgumentException("Argument 'element' was null.");

            int index = this.GetEntries().IndexOf(element);

            if (index == -1)
                return false;

            BinaryWriter writer = this.Disk.getWriter();

            if (GetNoBlocks(this.Disk, 4 * this.noEntries - 4) < this.NoBlocks)
            {
                // remove last block
                this.Disk.free(this.Inodes.Last().Address);
                this.Inodes.RemoveAt(this.Inodes.Count - 1);
                this.Inodes.Last().NextBlock = null;

                writer.Seek(this.Disk, Inodes.Last().Address);// noBlocks is written later
                writer.Write(0);
            }

            // Remove from disk (rewrite from index till last)
            // TODO: Please check for index errors. There are probably some in this part.
            int head = HeaderSize, noSubs, pos = index;
            Block current = this.Inodes.First();
            while ((noSubs = (this.Disk.BlockSize - head) / 4) <= pos)// find block
            {
                current = current.NextBlock;
                pos -= noSubs;
                head = SmallHeaderSize;
            }
            writer.Seek(this.Disk, current.Address, head + pos * 4);
            index++; // Skip removed element
            while (index < this.noEntries)
            {
                writer.Write(this.elements[index].Address);
                index++;
                pos++;

                // current block exhausted?
                if (pos >= (this.Disk.BlockSize - head) / 4 && index < this.noEntries)
                {
                    pos = 0;
                    current = current.NextBlock;
                    head = SmallHeaderSize;
                    writer.Seek(this.Disk, current.Address, head);
                }
            }

            this.elements.Remove(element);
            this.noEntries = this.elements.Count;
            writer.Seek(this.Disk, this.Address, 8);// Update noEntries
            writer.Write(this.noEntries);
            if (this.Inodes.Count != this.NoBlocks) // Update noBlocks
            {
                this.NoBlocks = this.Inodes.Count;
                writer.Write(this.NoBlocks);
            }
            return true;
        }
    }
}