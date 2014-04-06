using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFS.VFS.Extensions;

namespace VFS.VFS.Models
{
    /*
 * NextBlock 4
 * Parent 4
 * NrOfChildren 4
 * NoBlocks 4
 * Directory 1
 * NameSize 1
 * Name 110
 * Children x * 4
 */

    public class VfsDirectory : VfsFile
    {
        public VfsDirectory(VfsDisk disk, int address, string name, VfsDirectory parent, long noEntries, int noBlocks, int nextBlock)
            : base(disk, address, name, parent, noEntries, noBlocks, nextBlock)
        {
            IsDirectory = true;
        }

        /// <summary>
        /// Do not use, for Load method only. Use GetEntries() instead (this makes sure it's loaded).
        /// </summary>
        private List<VfsEntry> elements;
        private long noEntries
        {
            get { return FileSize; }
            set { FileSize = value; }
        }

        /// <summary>
        /// Loads the directory, fills Inodes and Elements. Should only be called in getEntries();
        /// </summary>
        private void Load()
        {
            if (noEntries == 0)
            {
                Inodes = new List<Block> { new Block(Address, null) };
                elements = new List<VfsEntry>();
                IsLoaded = true;
                return;
            }


            BinaryReader reader = Disk.GetReader;
            Inodes = new List<Block> { new Block(Address, null) };
            List<int> elementAddresses = new List<int>();
            int head = HeaderSize, doneEntriesInCurrentBlock = 0, totalEntries = 0;
            int nextBlock = NextBlock;

            reader.Seek(Disk, Address, head);
            while (totalEntries < noEntries)
            {
                elementAddresses.Add(reader.ReadInt32());
                totalEntries++;
                doneEntriesInCurrentBlock++;

                // Current Block exhausted?
                if (doneEntriesInCurrentBlock >= (Disk.BlockSize - head) / 4 && totalEntries != noEntries)
                {
                    Block next = new Block(nextBlock, null);
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

            elements = elementAddresses.Select(address => EntryFactory.OpenEntry(Disk, address, this)).ToList();

            IsLoaded = true;
        }

        /// <summary>
        /// Returns the entries contained in this directory and loads it if it was unloaded before.
        /// </summary>
        /// <returns>Returns the entries contained in this directory.</returns>
        public IEnumerable<VfsEntry> GetEntries
        {
            get
            {
                if (!IsLoaded)
                    Load();

                return elements;
            }
        }
        /// <summary>
        /// Looks for an entry with a given name.
        /// </summary>
        /// <returns>VfsEntry if one existed, otherwise null.</returns>
        public VfsEntry GetEntry(string name)
        {
            return GetEntries.FirstOrDefault(entry => entry.Name.Equals(name));
        }

        /// <summary>
        /// Lists all SubDirectories.
        /// </summary>
        /// <returns>An enumeration of the subdirectories.</returns>
        public IEnumerable<VfsDirectory> GetDirectories
        {
            get
            {
                return GetEntries.Where(el => el.IsDirectory).Cast<VfsDirectory>();
            }
        }
        /// <summary>
        /// Finds a directory with a given name.
        /// </summary>
        /// <param name="name">The Name.</param>
        /// <returns>Returns the VfsDirectory, if there exists one with the given name, othewise null.</returns>
        public VfsDirectory GetDirectory(string name)
        {
            VfsEntry e = GetEntries.FirstOrDefault(entry => entry.Name == name);
            if (e != null && e.IsDirectory)
                return (VfsDirectory)e;
            else
                return null;
        }

        /// <summary>
        /// Lists all contained files.
        /// </summary>
        /// <returns>An enumeration of the contained files.</returns>
        public IEnumerable<VfsFile> GetFiles
        {
            get
            {
                return GetEntries.Where(el => !el.IsDirectory).Cast<VfsFile>();
            }
        }

        /// <summary>
        /// Finds a file with a given name.
        /// </summary>
        /// <param name="name">The Name.</param>
        /// <returns>Returns the VfsFile, if there exists one with the given name, otherwise null.</returns>
        public VfsFile GetFile(string name)
        {
            VfsEntry e = GetEntries.FirstOrDefault(entry => entry.Name == name);
            if (e != null && !e.IsDirectory)
                return (VfsFile)e;
            else
                return null;
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

            if (element.Disk != Disk)
                throw new ArgumentException("Can't add an element from another disk to this directory.");

            if (!IsLoaded)
                Load();

            if (elements.Contains(element))
                return false;

            BinaryWriter writer = Disk.GetWriter;

            if (GetNoBlocks(Disk, 4 * noEntries + 4) > NoBlocks)
            {// add a new block
                int address;
                if (!Disk.Allocate(out address))
                    throw new ArgumentException("There is not enough space on this disk to add a new File to this directory!");
                Block block = new Block(address, null), last = Inodes.Last();
                last.NextBlock = block;
                Inodes.Add(block);

                writer.Seek(Disk, last.Address);
                writer.Write(address);
                writer.Seek(Disk, address);
                writer.Write(0);
                writer.Write(Address);
            }
            else
            {
                // seek to end of used content
                // TODO: Please check really hard for index errors, there are probably some in here.
                int head = HeaderSize, noSubs, pos = (int)noEntries;
                Block current = Inodes.First();
                while ((noSubs = (Disk.BlockSize - head) / 4) <= pos)// find block
                {
                    current = current.NextBlock;
                    pos -= noSubs;
                    head = SmallHeaderSize;
                }
                writer.Seek(Disk, current.Address, head + pos * 4);
            }

            writer.Write(element.Address);

            elements.Add(element);
            element.Parent = this;
            noEntries = elements.Count;
            writer.Seek(Disk, Address, FileOffset.FileSize);// Update noEntries
            writer.Write(noEntries);
            if (Inodes.Count != NoBlocks) // Update noBlocks
            {
                NoBlocks = Inodes.Count;
                writer.Write(NoBlocks);
            }
            writer.Flush();
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

            int index = GetEntries.ToList().IndexOf(element);

            if (index == -1)
                return false;

            BinaryWriter writer = Disk.GetWriter;

            if (GetNoBlocks(Disk, 4 * noEntries - 4) < NoBlocks)
            {
                // remove last block
                Disk.Free(Inodes.Last().Address);
                Inodes.RemoveAt(Inodes.Count - 1);
                Inodes.Last().NextBlock = null;

                writer.Seek(Disk, Inodes.Last().Address);// noBlocks is written later
                writer.Write(0);
            }

            // Remove from disk (rewrite from index till last)
            // TODO: Please check for index errors. There are probably some in this part.
            int head = HeaderSize, noSubs, pos = index;
            Block current = Inodes.First();
            while ((noSubs = (Disk.BlockSize - head) / 4) <= pos)// find block
            {
                current = current.NextBlock;
                pos -= noSubs;
                head = SmallHeaderSize;
            }
            writer.Seek(Disk, current.Address, head + pos * 4);
            index++; // Skip removed element
            while (index < noEntries)
            {
                writer.Write(elements[index].Address);
                index++;
                pos++;

                // current block exhausted?
                if (pos >= (Disk.BlockSize - head) / 4 && index < noEntries)
                {
                    pos = 0;
                    current = current.NextBlock;
                    head = SmallHeaderSize;
                    writer.Seek(Disk, current.Address, head);
                }
            }

            elements.Remove(element);
            noEntries = elements.Count;
            writer.Seek(Disk, Address, FileOffset.FileSize);// Update noEntries
            writer.Write(noEntries);
            if (Inodes.Count != NoBlocks) // Update noBlocks
            {
                NoBlocks = Inodes.Count;
                writer.Write(NoBlocks);
            }
            writer.Flush();
            return true;
        }

        /// <summary>
        /// Calculates how much space remains in the currently allocated blocks.
        /// </summary>
        /// <returns>Returns the number of free entry-spaces.</returns>
        public int SpaceInAllocatedBlocks()
        {
            if (noEntries <= (Disk.BlockSize - HeaderSize) / 4)
                return (Disk.BlockSize - HeaderSize) / 4 - (int)noEntries;
            else
            {
                long remaining = noEntries - (Disk.BlockSize - HeaderSize)/4;
                int space = (Disk.BlockSize - SmallHeaderSize)/4;

                remaining -= space * (remaining / space);

                return space - (int)remaining;
            }
        }

        /// <summary>
        /// Returns 1 if the directory needs to allocate a new block if noe many entries would be added to it, 0 otherwise.
        /// </summary>
        /// <param name="noe">The number of entries.</param>
        /// <returns>1 or 0</returns>
        public int SpaceIndicator(int noe)
        {
            return noe > SpaceInAllocatedBlocks() ? 1 : 0;
        }

        public override string ToString()
        {
            if (IsLoaded)
                return "{Directory(loaded): Name:" + Name + " Children:" + noEntries + " Address:" + Address + " Blocks:" + NoBlocks + "}";
            else
                return "{Directory: Name:" + Name + " Children:" + noEntries + " Address:" + Address + " Blocks:" + NoBlocks + "}";
        }
    }
}