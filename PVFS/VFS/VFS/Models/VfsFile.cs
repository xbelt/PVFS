using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime.Atn;

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

        #endregion


        public bool IsDirectory { get; set; }
        public VfsDirectory Parent { get; set; }
        public string Name { get; set; }
        public string Type
        {
            get { return IsDirectory ? null : Name.Substring(Name.LastIndexOf(".") + 1); }
        }

        protected VfsDisk Disk;
        protected bool IsLoaded;
        protected int FileSize, NoBlocks, NextBlock;
        protected List<Block> Inodes;

        public VfsFile(VfsDisk disk, int address, string name, VfsDirectory parent, int filesize, int noBlocks,
            int nextBlock)
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

                reader.BaseStream.Seek(next.Address * Disk.BlockSize, SeekOrigin.Begin);
                nextAddress = reader.ReadInt32();
                if (reader.ReadInt32() != this.Address)
                    throw new IOException("The startBlock Address of block " + this.Inodes.Last().Address + " was inconsistent.");
            }
        }

        /// <summary>
        /// writes the content of a BinaryReader to this vFile.TODO: Extends file if thie content is longer than the original vFile.
        /// </summary>
        public void Write(BinaryReader reader)
        {
            //TODO: add extendability if file > vFile

            BinaryWriter writer = Disk.getWriter();

            if (!this.IsLoaded)
                this.Load();

            int blockId = 0, head = HeaderSize, totalSize = 0;
            byte[] buffer = new byte[Disk.BlockSize - SmallHeaderSize];
            while (blockId < Inodes.Count)
            {
                int count = reader.Read(buffer, 0, Disk.BlockSize - head);
                writer.Seek(Inodes[blockId].Address * Disk.BlockSize + head, SeekOrigin.Begin);
                writer.Write(buffer, 0, count);
                totalSize += count;
                if (count < Disk.BlockSize - head)
                    goto end;// LOL

                blockId++;
                head = SmallHeaderSize; // only first block has large header
            }
            //if we reach this then the file is larger than the vfile -> extend vfile
            //dont forget to update the file header - noBlocks!

            end://wait this really works? THIS IS INSANE
            if (totalSize != FileSize)
            {
                // write file size
                writer.Seek(this.Address * Disk.BlockSize + 8, SeekOrigin.Begin);
                writer.Write(totalSize);
            }
        }

        /// <summary>
        /// reads the content of this vFile and writes it to the supplied BinaryWriter.
        /// </summary>
        public void Read(BinaryWriter writer)
        {

        }

        /// <summary>
        /// move to VfsManager!
        /// </summary>
        /// <param name="path"></param>
        public override void Open(string path)
        {
            base.Open(path);
        }

        public void Open(int address)
        {
            //TODO: I am not yet sure how to get the parent directory
            //F: opening of files/directories will be done in VfsEntry/EntryFactory
            //F: maybe we don't need the fields disk, parent and directory.
            var reader = new BinaryReader(Disk.FileStream);
            var blockSize = Disk.DiskProperties.BlockSize;
            var buffer = new Byte[blockSize];
            reader.Read(buffer, address * blockSize, blockSize);
            var numberOfBlocks = BitConverter.ToInt32(buffer, 12);
            IsDirectory = BitConverter.ToBoolean(buffer, 16);
            Name = BitConverter.ToString(buffer, 18, 110);
            Inodes.Add(new Block(address, address, new Block(address, address, null)));
            var nextAddress = BitConverter.ToInt32(buffer, 0);

            for (int i = 0; i < numberOfBlocks - 1; i++)
            {
                reader.Read(buffer, nextAddress * blockSize, blockSize);
                var nextBlock = new Block(nextAddress, address, null);
                Inodes.Last().NextBlock = nextBlock;
                Inodes.Add(nextBlock);
                nextAddress = BitConverter.ToInt32(buffer, 0);
            }
        }
    }
}
