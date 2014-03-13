using System;
using System.IO;

namespace VFS.VFS.Models {
    public abstract class VfsEntry {
        public int Address { get; set; }

        /// <summary>
        /// move to VFS Manager?
        /// </summary>
        public virtual void Open(string path) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// move to EntryFactory?
        /// </summary>
        public static VfsFile Open(int address, VfsDisk disk, BinaryWriter writer)
        {
            BinaryReader reader = disk.getReader();
            reader.BaseStream.Seek(address * disk.BlockSize, SeekOrigin.Begin);
            int nextBlock = reader.ReadInt32();
            int startBlock = reader.ReadInt32();
            if (startBlock != address)
                throw new ArgumentException("Address is not a startblock of a file or directory!");
            int fileSize = reader.ReadInt32();//fileSize doubles ad noEntries for a directory!
            int noBlocks = reader.ReadInt32();
            bool directory = reader.ReadBoolean();
            byte nameSize = reader.ReadByte();
            string name = new string(reader.ReadChars(nameSize));
            if (directory)
            {
                return new VfsDirectory(disk, null, name, address, fileSize, noBlocks, nextBlock);
            }
            else
            {
                return new VfsFile(disk, address, name, null, fileSize, noBlocks, nextBlock);
            }
        }
    }
}