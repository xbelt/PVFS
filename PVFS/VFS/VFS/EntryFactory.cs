using System;
using System.Collections.Generic;
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

    public class EntryFactory : Factory
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
            if (disk == null) 
                throw new ArgumentNullException("disk");
            var reader = disk.GetReader();
            reader.Seek(disk, address);

            var nextBlock = reader.ReadInt32();

            var fileSize = reader.ReadInt64();//fileSize doubles as noEntries for a directory!
            var noBlocks = reader.ReadInt32();
            var directory = reader.ReadBoolean();
            reader.ReadInt32();
            var nameSize = reader.ReadByte();
            var name = new string(reader.ReadChars(nameSize));

            return directory ? new VfsDirectory(disk, address, name, parent, fileSize, noBlocks, nextBlock) : new VfsFile(disk, address, name, parent, fileSize, noBlocks, nextBlock);
        }

        /// <summary>
        /// creates a file on the disk with the specified size, with random content.
        /// Make sure that parent does not already contain a file with the same name.
        /// throws exceptions if invalid name/invalid size/disk too full
        /// </summary>
        /// <returns>a handle for the file</returns>
        public static VfsFile createFile(VfsDisk disk, string name, long size, VfsDirectory parent)
        {
            if (disk == null)
                throw new ArgumentNullException("disk");
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length > VfsFile.MaxNameLength)
                throw new ArgumentException("The filename can't be longer than " + VfsFile.MaxNameLength + ".");
            if (size < 0)
                throw new ArgumentException("Can't create files larger than 1 Gb.");
            if (parent == null)
                throw new ArgumentNullException("parent");
            int[] addresses;
            if (!disk.Allocate(out addresses, VfsFile.GetNoBlocks(disk, size)))
                throw new ArgumentException("There is not enough space on this disk!");
            var blocks = new List<Block>();
            var writer = disk.GetWriter();

            // Write File Header
            writer.Seek(disk, addresses[0]);
            writer.Write(addresses.Length == 1 ? 0 : addresses[1]);
            writer.Write(size);
            writer.Write(addresses.Length);
            writer.Write(false);
            writer.Write(parent.Address);
            writer.Write((byte)name.Length);
            writer.Write(name.ToCharArray());

            blocks.Add(new Block(addresses[0], null));

            for (var i = 1; i < addresses.Length; i++)
            {
                // Write block header
                writer.Seek(disk, addresses[i]);
                writer.Write(i == addresses.Length - 1 ? 0 : addresses[i + 1]);

                blocks.Add(new Block(addresses[i], blocks.Last()));
            }
            writer.Flush();
            var vfsFile = new VfsFile(disk, addresses[0], name, parent, size, blocks);
            parent.AddElement(vfsFile);
            return vfsFile;
        }

        /// <summary>
        /// creates an empty directory on the disk
        /// throws exception if the disk is too full/invalid name
        /// </summary>
        /// <returns>a handle for the directory</returns>
        public static VfsDirectory createDirectory(VfsDisk disk, string name, VfsDirectory parent)
        {
            if (disk == null)
                throw new ArgumentNullException("disk");
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length > VfsFile.MaxNameLength)
                throw new ArgumentException("The directory-name can't be longer than " + VfsFile.MaxNameLength + ".");
            int address;
            if (!disk.Allocate(out address))
                throw new ArgumentException("There is not enough place on this disk!");
            if (parent == null)
                throw new ArgumentNullException("parent");

            var writer = disk.GetWriter();

            // Write Directory Header
            writer.Seek(disk, address);
            writer.Write(0);
            writer.Write(0L);
            writer.Write(1);
            writer.Write(true);
            writer.Write(parent.Address);
            writer.Write((byte)name.Length);
            writer.Write(name.ToCharArray());
            writer.Flush();

            var vfsDirectory = new VfsDirectory(disk, address, name, parent, 0, 1, 0);
            parent.AddElement(vfsDirectory);
            return vfsDirectory;
        }
    }
}
