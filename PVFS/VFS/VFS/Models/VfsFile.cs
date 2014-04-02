using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFS.VFS.Extensions;

namespace VFS.VFS.Models
{
    /*                  Range
     * NextBlock 4      0-3
     * StartBlock 4     4-7
     * FileSize 4       8-11
     * NoBlocks 4       12-15
     * Directory 1      16
     * ParentAddress 4  17
     * NameSize 1       21
     * Name 106         22-128
     * Data
     */

    public class VfsFile : VfsEntry
    {
        #region Const

        /// <summary>
        /// Block-Header size of a startblock of a file/directory
        /// </summary>
        public const int HeaderSize = FileOffset.Header;

        /// <summary>
        /// Block-Header size of a non-startblock of a file/directory
        /// </summary>
        public const int SmallHeaderSize = FileOffset.SmallHeader;

        /// <summary>
        /// Maximal File size
        /// </summary>
        // TODO: correctly determine maxSize with blockSize
        public const int MaxSize = 1024 * 1024 * 1024; // 1Gb

        /// <summary>
        /// Maximal File/Directory Name Length
        /// </summary>
        public const int MaxNameLength = 110;

        #endregion

        public VfsDirectory Parent { get; protected internal set; }
        public string Type
        {
            get
            {
                if (Name.Length > Name.LastIndexOf(".") + 1)
                    return IsDirectory ? null : Name.Substring(Name.LastIndexOf(".") + 1);
                else
                    return null;
            }
        }

        protected internal VfsDisk Disk { get; protected set; }
        protected bool IsLoaded;
        protected int FileSize, NoBlocks, NextBlock;
        protected List<Block> Inodes;

        #region Constructor

        /// <summary>
        /// Constructs an unloaded file.
        /// </summary>
        public VfsFile(VfsDisk disk, int address, string name, VfsDirectory parent, int filesize, int noBlocks, int nextBlock)
        {
            if (disk == null)
                throw new ArgumentNullException("disk");
            if (name == null)
                throw new ArgumentNullException("name");


            Disk = disk;
            Address = address;
            Name = name;
            IsDirectory = false;
            IsLoaded = false;
            Parent = parent;
            FileSize = filesize;
            NoBlocks = noBlocks;
            NextBlock = nextBlock;
        }

        /// <summary>
        /// if we already know all blocks, use this (this happens when we just allocated a new file)
        /// Constructs a Loaded File
        /// </summary>
        public VfsFile(VfsDisk disk, int address, string name, VfsDirectory parent, int filesize, List<Block> blocks)
        {
            if (disk == null)
                throw new ArgumentNullException("disk");
            if (name == null)
                throw new ArgumentNullException("name");
            if (blocks == null)
                throw new ArgumentNullException("blocks");
            if (blocks.Count == 0)
                throw new ArgumentException("Blocks can't be empty.");

            Disk = disk;
            Address = address;
            Name = name;
            IsDirectory = false;
            IsLoaded = true;
            Parent = parent;
            FileSize = filesize;
            NoBlocks = blocks.Count;
            NextBlock = NoBlocks > 1 ? blocks[1].Address : 0;
            Inodes = blocks;
        }

        #endregion

        /// <summary>
        /// used in write, read and free to load the blocks
        /// </summary>
        private void Load()
        {
            var reader = Disk.getReader();
            Inodes = new List<Block> { new Block(Address,  null) };
            var nextAddress = NextBlock;

            for (var i = 0; i < NoBlocks - 1; i++)
            {
                var next = new Block(nextAddress, null);
                Inodes.Last().NextBlock = next;
                Inodes.Add(next);

                reader.Seek(Disk, nextAddress);
                nextAddress = reader.ReadInt32();
            }
            if (nextAddress != 0)
                throw new IOException("The nextBlock Address of block " + Inodes.Last().Address + " is not 0 (it's the last block).");

            IsLoaded = true;
        }


        /// <summary>
        /// writes the content of a BinaryReader to this vFile. Extends file if thie content is longer than the original vFile.
        /// </summary>
        public void Write(BinaryReader reader)
        {
            if (this.IsDirectory)
                throw new ArgumentException("Can't be called on a directory.");

            var writer = Disk.getWriter();

            if (!IsLoaded)
                Load();

            int blockId = 0, head = HeaderSize, totalSize = 0, count;
            var buffer = new byte[Disk.BlockSize - SmallHeaderSize];
            while (blockId < Inodes.Count)
            {
                count = reader.Read(buffer, 0, Disk.BlockSize - head);
                writer.Seek(Disk, Inodes[blockId].Address, head);
                writer.Write(buffer, 0, count);
                totalSize += count;

                if (count < Disk.BlockSize - head)
                    break;

                blockId++;
                head = SmallHeaderSize; // only first block has large header
            }

            // If the file is larger than the initially allocated vFile: extend the vFile
            while ((count = reader.Read(buffer, 0, Disk.BlockSize - SmallHeaderSize)) > 0)
            {
                int address;
                if (!Disk.Allocate(out address))
                    throw new ArgumentException("There is not enough space on this disk!");

                var last = Inodes.Last(); // This is inefficient (the write head jumps back and forth to fill nextBlock-address)
                writer.Seek(Disk, last.Address);
                writer.Write(address);
                last.NextBlock = new Block(address, null);
                Inodes.Add(last.NextBlock);

                writer.Seek(Disk, address);
                writer.Write(0);// nextBlock unknown
                writer.Write(Address);
                writer.Write(buffer, 0, count);
                totalSize += count;
            }

            // Update Header
            if (Inodes.Count != NoBlocks)
            {
                NoBlocks = Inodes.Count;
                writer.Seek(Disk, Address, FileOffset.NumberOfBlocks);
                writer.Write(NoBlocks);
            }

            if (totalSize != FileSize)
            {
                FileSize = totalSize;
                writer.Seek(Disk, Address, FileOffset.FileSize);
                writer.Write(totalSize);
            }
            writer.Flush();
        }

        /// <summary>
        /// reads the content of this vFile and writes it to the supplied BinaryWriter.
        /// </summary>
        public void Read(BinaryWriter writer)
        {
            if (this.IsDirectory)
                throw new ArgumentException("Can't be called on a directory.");

            var reader = Disk.getReader();

            if (!IsLoaded)
                Load();

            int blockId = 0, head = HeaderSize, totalRead = 0;
            var buffer = new byte[Disk.BlockSize - SmallHeaderSize];
            while (blockId < Inodes.Count)
            {
                reader.Seek(Disk, Inodes[blockId].Address, head);
                reader.Read(buffer, 0, Disk.BlockSize - head);

                var count = Disk.BlockSize - head;
                if (count > FileSize - totalRead)
                    count = FileSize - totalRead;

                writer.Write(buffer, 0, count);

                totalRead += count;
                blockId++;
                head = SmallHeaderSize; // only first block has large header
            }
        }

        /// <summary>
        /// Deallocates all Blocks.
        /// </summary>
        public void Free()
        {
            if (!IsLoaded)
                Load();

            foreach (var inode in Inodes)
            {
                Disk.Free(inode.Address);
            }

            Inodes = null;
            IsLoaded = false;
        }

        /// <summary>
        /// Renames the File/Directory and updates the disk.
        /// </summary>
        /// <param name="name">The new Name.</param>
        public void Rename(string name)
        {
            if (name == null)
                throw new ArgumentException("Name was null.");
            if (name.Length > MaxNameLength)
                throw new ArgumentException("Name was too long.");

            BinaryWriter writer = Disk.getWriter();
            writer.Seek(Disk, Address, FileOffset.NameLength);
            writer.Write((byte)name.Length);
            writer.Write(name.ToCharArray());
            writer.Flush();
            Name = name;
        }

        /// <summary>
        /// Returns the absolute path to this file/directory.
        /// File Format: /DiskName/DirectoryName/.../Filename.exe
        /// Directory Format: /DiskName/DirectoryName/.../DirectoryName
        /// </summary>
        /// <returns>Returns the absolute path to this file/directory.</returns>
        public string GetAbsolutePath()
        {
            if (Parent != null)
            {
                return Parent.GetAbsolutePath() + "/" + Name;
            }
            return "/" + this.Disk.DiskProperties.Name;
        }

        /// <summary>
        /// Copies this file into a destination directory with a given name.
        /// Please check that nthere is no other file/dir in the destination with the same name.
        /// </summary>
        /// <param name="destination">The directory into which the copy goes.</param>
        /// <param name="copyName">The name of the copied file.</param>
        /// <returns>Returns the newly created file which is a copy of this.</returns>
        public VfsFile Duplicate(VfsDirectory destination, string copyName)
        {
            if (!IsLoaded)
                Load();
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (copyName == null)
                throw new ArgumentNullException("copyName");
            if (destination.GetEntry(copyName) != null)
                throw new ArgumentException("Destination already contained a file with name " + copyName + ". Add a check to VfsManager.copy().");

            if (this.IsDirectory)
                throw new ArgumentException("This can't be called on a directory!");

            VfsFile copy = EntryFactory.createFile(this.Disk, this.Name, this.FileSize, destination);

            BinaryReader reader = this.Disk.getReader();
            BinaryWriter writer = this.Disk.getWriter();
            byte[] buffer = new byte[this.Disk.BlockSize - SmallHeaderSize];
            int head = HeaderSize;

            for (int i = 0; i < this.Inodes.Count; i++)
            {
                reader.Seek(this.Disk, this.Inodes[i].Address, head);
                reader.Read(buffer, 0, this.Disk.BlockSize - head);

                writer.Seek(this.Disk, copy.Inodes[i].Address, head);
                writer.Write(buffer, 0, this.Disk.BlockSize - head);

                head = SmallHeaderSize;
            }
            writer.Flush();
            return copy;
        }

        /// <summary>
        /// Returns the total number of blocks needed for a file of specified size on the target disk. This accounts for the startblock.
        /// </summary>
        /// <param name="disk">the target disk</param>
        /// <param name="filesize">thie FileSize in Bytes</param>
        /// <returns>number of blocks (Startblock + Following blocks) needed for this file</returns>
        public static int GetNoBlocks(VfsDisk disk, int filesize)
        {
            var blockSize = disk.BlockSize;
            if (filesize > blockSize - HeaderSize)
            {
                filesize -= blockSize - HeaderSize;
                var noBlocks = filesize / (blockSize - SmallHeaderSize);
                if (noBlocks * (blockSize - SmallHeaderSize) != filesize) noBlocks++;
                return noBlocks + 1;
            }
            return 1;
        }

        public override string ToString()
        {
            if (IsLoaded)
                return "{File(loaded): Address:" + Address + " Size:" + FileSize + " Blocks:" + NoBlocks + "}";
            else
                return "{FIle: Address:" + Address + " Size:" + FileSize + " Blocks:" + NoBlocks + "}";
        }
    }
}
