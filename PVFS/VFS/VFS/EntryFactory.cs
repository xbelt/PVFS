using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFS.VFS.Extensions;
using VFS.VFS.Models;

namespace VFS.VFS
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
    class EntryFactory : Factory
    {
        /// <summary>
        /// Opens a startblock of a File or Directory and creates the corresponding VfsEntry.
        /// </summary>
        /// <param name="disk">The disk the entry is on</param>
        /// <param name="address">The address of the startblock</param>
        /// <param name="parent">The parent folder of the entry</param>
        /// <returns>A new VfsFile or VfsDirectory</returns>
        public static VfsEntry OpenEntry(VfsDisk disk, int address, VfsDirectory parent)
        {
            BinaryReader reader = disk.getReader();
            reader.Seek(disk, address);

            int nextBlock = reader.ReadInt32();
            int startBlock = reader.ReadInt32();
            if (startBlock != address)
                throw new ArgumentException("Address is not a startblock of a file or directory!");

            int fileSize = reader.ReadInt32();//fileSize doubles as noEntries for a directory!
            int noBlocks = reader.ReadInt32();
            bool directory = reader.ReadBoolean();
            byte nameSize = reader.ReadByte();
            string name = new string(reader.ReadChars(nameSize));

            if (directory)
            {
                return new VfsDirectory(disk, address, name, parent, fileSize, noBlocks, nextBlock);
            }
            else
            {
                return new VfsFile(disk, address, name, parent, fileSize, noBlocks, nextBlock);
            }
        }

        /// <summary>
        /// creates a file on the disk with the specified size, with random content
        /// throws exceptions if invalid name/invalid size/disk too full
        /// </summary>
        /// <returns>a handle for the file</returns>
        public VfsFile createFile(VfsDisk disk, string name, int size, VfsDirectory parent)
        {
            if (name.Length > VfsFile.MaxNameLength)
                throw new ArgumentException("The filename can't be longer than " + VfsFile.MaxNameLength + ".");
            if (size < 0 || size > VfsFile.MaxSize)
                throw new ArgumentException("Can't create files larger than 1 Gb.");
            int[] addresses;
            if (!disk.allocate(out addresses, VfsFile.getNoBlocks(disk, size)))
                throw new ArgumentException("There is not enough space on this disk!");
            List<Block> blocks = new List<Block>();
            BinaryWriter writer = disk.getWriter();

            // Write File Header
            //(Might want to export this in a single function?)
            //TODO: apparently wrong parameter for Seek?
            writer.Seek(disk, addresses[0]);
            writer.Write(addresses.Length == 1 ? 0 : addresses[1]);
            writer.Write(addresses[0]);
            writer.Write(size);
            writer.Write(addresses.Length);
            writer.Write(false);
            writer.Write((byte)name.Length);
            writer.Write(name.ToCharArray());

            blocks.Add(new Block(addresses[0], addresses[0], null));

            for (int i = 1; i < addresses.Length; i++)
            {
                // Write block header
                //TODO: apparently wrong parameter for Seek?
                writer.Seek(disk, addresses[i]);
                writer.Write(i == addresses.Length - 1 ? 0 : addresses[i + 1]);
                writer.Write(addresses[0]);

                blocks.Add(new Block(addresses[i], addresses[0], blocks.Last()));
            }
            return new VfsFile(disk, addresses[0], name, parent, size, blocks);
        }

        /// <summary>
        /// creates an empty directory on the disk
        /// throws exception if the disk is too full/invalid name
        /// </summary>
        /// <returns>a handle for the directory</returns>
        public VfsDirectory createDirectory(VfsDisk disk, string name, VfsDirectory parent)
        {
            //TODO: prevent creation if directory with same name already exists (here)
            if (name.Length > VfsFile.MaxNameLength)
                throw new ArgumentException("The directory-name can't be longer than " + VfsFile.MaxNameLength + ".");
            int address;
            if (!disk.allocate(out address))
                throw new ArgumentException("There is not enough place on this disk!");
            BinaryWriter writer = disk.getWriter();

            // Write Directory Header
            //TODO: apparently wrong parameter for Seek?
            writer.Seek(disk, address);
            writer.Write(0);
            writer.Write(address);
            writer.Write(0);
            writer.Write(1);
            writer.Write(false);
            writer.Write((byte)name.Length);
            writer.Write(name.ToCharArray());

            return new VfsDirectory(disk, address, name, parent, 0, 1, 0);
        }
    }
}
