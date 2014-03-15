using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFS.VFS.Models;

namespace VFS.VFS.Extensions
{
    static class IOExtensions
    {
        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk.
        /// </summary>
        public static void Seek(this BinaryWriter writer, VfsDisk disk, int address)
        {
            writer.Seek(address * disk.BlockSize, SeekOrigin.Begin);
        }
        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk with an offset.
        /// </summary>
        public static void Seek(this BinaryWriter writer, VfsDisk disk, int address, int offset)
        {
            writer.Seek(address * disk.BlockSize + offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk.
        /// </summary>
        public static void Seek(this BinaryReader reader, VfsDisk disk, int address)
        {
            reader.BaseStream.Seek(address * disk.BlockSize, SeekOrigin.Begin);
        }
        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk with an offset.
        /// </summary>
        public static void Seek(this BinaryReader reader, VfsDisk disk, int address, int offset)
        {
            reader.BaseStream.Seek(address * disk.BlockSize + offset, SeekOrigin.Begin);
        }
    }
}
