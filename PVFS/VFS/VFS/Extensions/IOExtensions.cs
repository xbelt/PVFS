using System.IO;
using VFS.VFS.Models;

namespace VFS.VFS.Extensions
{
    public static class IOExtensions
    {
        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk.
        /// </summary>
        public static void Seek(this BinaryWriter writer, VfsDisk disk, int address)
        {
            if (writer != null) if (disk != null) writer.Seek(address * disk.BlockSize, SeekOrigin.Begin);
        }

        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk with an offset.
        /// </summary>
        public static void Seek(this BinaryWriter writer, VfsDisk disk, int address, int offset)
        {
            if (writer != null) if (disk != null) writer.Seek(address * disk.BlockSize + offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk.
        /// </summary>
        public static void Seek(this BinaryReader reader, VfsDisk disk, int address)
        {
            if (reader != null) if (disk != null) reader.BaseStream.Seek(address * disk.BlockSize, SeekOrigin.Begin);
        }

        /// <summary>
        /// Sets the position in the current stream according to the block address of the disk with an offset.
        /// </summary>
        public static void Seek(this BinaryReader reader, VfsDisk disk, int address, int offset)
        {
            if (reader != null)
                if (disk != null) reader.BaseStream.Seek(address * disk.BlockSize + offset, SeekOrigin.Begin);
        }
    }
}
