﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFS.VFS.Extensions;

namespace VFS.VFS.Models
{
    /*
     * NextBlock 4
     * StartBlock 4
     * FileSize 4
     * NoBlocks 4
     * Directory 1
     * NameSize 1
     * Name 110
     * Data
     */

    public class VfsFile : VfsEntry
    {
        #region Const

        /// <summary>
        /// Block-Header size of a startblock of a file/directory
        /// </summary>
        public const int HeaderSize = 128;

        /// <summary>
        /// Block-Header size of a non-startblock of a file/directory
        /// </summary>
        public const int SmallHeaderSize = 8;

        /// <summary>
        /// Maximal File size
        /// </summary>
        public const int MaxSize = 1024 * 1024 * 1024; // 1Gb

        /// <summary>
        /// Maximal File/Directory Name Length
        /// </summary>
        public const int MaxNameLength = 110;

        #endregion

        public VfsDirectory Parent { get; protected set; }
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

        protected VfsDisk Disk;
        protected bool IsLoaded;
        protected int FileSize, NoBlocks, NextBlock;
        protected List<Block> Inodes;

        #region Constructor

        /// <summary>
        /// Constructs an unloaded file.
        /// </summary>
        public VfsFile(VfsDisk disk, int address, string name, VfsDirectory parent, int filesize, int noBlocks, int nextBlock)
        {
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
            Inodes = new List<Block> { new Block(Address, Address, null) };
            var nextAddress = NextBlock;

            for (var i = 0; i < NoBlocks - 1; i++)
            {
                var next = new Block(nextAddress, Address, null);
                Inodes.Last().NextBlock = next;
                Inodes.Add(next);

                reader.Seek(Disk, nextAddress);
                nextAddress = reader.ReadInt32();
                if (reader.ReadInt32() != Address)
                    throw new IOException("The startBlock Address of block " + Inodes.Last().Address + " was inconsistent.");
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
            //TODO: add extendability if file > vFile

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
                if (!Disk.allocate(out address))
                    throw new ArgumentException("There is not enough space on this disk!");

                var last = Inodes.Last(); // This is inefficient (the write head jumps back and forth to fill nextBlock-address)
                writer.Seek(Disk, last.Address);
                writer.Write(address);
                last.NextBlock = new Block(address, Address, null);
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
                writer.Seek(Disk, Address, 12);
                writer.Write(NoBlocks);
            }

            if (totalSize != FileSize)
            {
                FileSize = totalSize;
                writer.Seek(Disk, Address, 8);
                writer.Write(totalSize);
            }
        }

        /// <summary>
        /// reads the content of this vFile and writes it to the supplied BinaryWriter.
        /// </summary>
        public void Read(BinaryWriter writer)
        {
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
                Disk.free(inode.Address);
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

            BinaryWriter writer = this.Disk.getWriter();
            writer.Seek(this.Disk, this.Address, 14);
            writer.Write(name.ToCharArray());
            this.Name = name;
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
    }
}
