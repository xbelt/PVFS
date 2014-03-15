using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime.Atn;
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
        public string Name { get; protected set; }
        public string Type
        {
            get { return IsDirectory ? null : Name.Substring(Name.LastIndexOf(".") + 1); }
        }

        protected VfsDisk Disk;
        protected bool IsLoaded;
        protected int FileSize, NoBlocks, NextBlock;
        protected List<Block> Inodes;

        #region Constructor

        public VfsFile(VfsDisk disk, int address, string name, VfsDirectory parent, int filesize, int noBlocks, int nextBlock)
        {
            this.Disk = disk;
            this.Address = address;
            this.Name = name;
            this.IsDirectory = false;
            this.IsLoaded = false;
            this.Parent = parent;
            this.FileSize = filesize;
            this.NoBlocks = noBlocks;
            this.NextBlock = nextBlock;
        }

        /// <summary>
        /// if we already know all blocks, use this (this happens when we just allocated a new file)
        /// </summary>
        public VfsFile(VfsDisk disk, int address, string name, VfsDirectory parent, int filesize, List<Block> blocks)
        {
            this.Disk = disk;
            this.Address = address;
            this.Name = name;
            this.IsDirectory = false;
            this.IsLoaded = true;
            this.Parent = parent;
            this.FileSize = filesize;
            this.NoBlocks = blocks.Count;
            this.NextBlock = NoBlocks > 0 ? blocks[1].Address : 0;
            this.Inodes = blocks;
        }

        #endregion


        /// <summary>
        /// used in write and read to load the blocks
        /// </summary>
        private void Load()
        {
            BinaryReader reader = Disk.getReader();
            this.Inodes = new List<Block> { new Block(this.Address, this.Address, null) };
            int nextAddress = this.NextBlock;

            for (int i = 0; i < NoBlocks - 1; i++)
            {
                Block next = new Block(nextAddress, this.Address, null);
                this.Inodes.Last().NextBlock = next;
                this.Inodes.Add(next);

                reader.Seek(Disk, next.Address);
                nextAddress = reader.ReadInt32();
                if (reader.ReadInt32() != this.Address)
                    throw new IOException("The startBlock Address of block " + this.Inodes.Last().Address + " was inconsistent.");
            }
            if (nextAddress != 0)
                throw new IOException("The nextBlock Address of block " + this.Inodes.Last().Address + " was not 0 (it's the last block).");
        }

        /// <summary>
        /// writes the content of a BinaryReader to this vFile. Extends file if thie content is longer than the original vFile.
        /// </summary>
        public void Write(BinaryReader reader)
        {
            //TODO: add extendability if file > vFile

            BinaryWriter writer = Disk.getWriter();

            if (!this.IsLoaded)
                this.Load();

            int blockId = 0, head = HeaderSize, totalSize = 0, count;
            byte[] buffer = new byte[Disk.BlockSize - SmallHeaderSize];
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

                Block last = Inodes.Last(); // This is inefficient (the write head jumps back and forth to fill nextBlock-address)
                writer.Seek(Disk, last.Address);
                writer.Write(address);
                last.NextBlock = new Block(address, this.Address, null);
                Inodes.Add(last.NextBlock);

                writer.Seek(Disk, address);
                writer.Write(0);// nextBlock unknown
                writer.Write(this.Address);
                writer.Write(buffer, 0, count);
                totalSize += count;
            }

            // Update Header
            if (Inodes.Count != NoBlocks)
            {
                this.NoBlocks = Inodes.Count;
                writer.Seek(Disk, this.Address, 12);
                writer.Write(this.NoBlocks);
            }

            if (totalSize != FileSize)
            {
                this.FileSize = totalSize;
                writer.Seek(Disk, this.Address, 8);
                writer.Write(totalSize);
            }
        }

        /// <summary>
        /// reads the content of this vFile and writes it to the supplied BinaryWriter.
        /// </summary>
        public void Read(BinaryWriter writer)
        {
            BinaryReader reader = Disk.getReader();

            if (!this.IsLoaded)
                this.Load();

            int blockId = 0, head = HeaderSize, totalRead = 0;
            byte[] buffer = new byte[Disk.BlockSize - SmallHeaderSize];
            while (blockId < Inodes.Count)
            {
                reader.Seek(Disk, Inodes[blockId].Address, head);
                reader.Read(buffer, 0, Disk.BlockSize - head);

                int count = Disk.BlockSize - head;
                if (count > FileSize - totalRead)
                    count = FileSize - totalRead;

                writer.Write(buffer, 0, count);

                totalRead += count;
                blockId++;
                head = SmallHeaderSize; // only first block has large header
            }
        }


        /// <summary>
        /// Returns the total number of blocks needed for a file of specified size on the target disk. This accounts for the startblock.
        /// </summary>
        /// <param name="disk">the target disk</param>
        /// <param name="filesize">thie FileSize in Bytes</param>
        /// <returns>number of blocks (Startblock + Following blocks) needed for this file</returns>
        public static int getNoBlocks(VfsDisk disk, int filesize)
        {
            int blockSize = disk.BlockSize;
            if (filesize > blockSize - HeaderSize)
            {
                filesize -= blockSize - HeaderSize;
                int noBlocks = filesize / (blockSize - SmallHeaderSize);
                if (noBlocks * (blockSize - SmallHeaderSize) != filesize) noBlocks++;
                return noBlocks + 1;
            }
            else return 1;
        }
    }
}
