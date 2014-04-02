using System;
using System.IO;
using System.Text;

namespace VFS.VFS.Models {
    public class DiskProperties {
        /// <summary>
        /// The name of the disk without its .vdi ending
        /// </summary>
        public string Name { get; set; }
        public int NumberOfBlocks { get; set; }
        public int NumberOfUsedBlocks { get; set; }

        public static int BitMapOffset
        {
            get
            {
                return (4 + 4 + 4 + 8 + 4 + 4 + 4*128);
            }
        }

        public double MaximumSize { get; set; }
        public int BlockSize { get; set; }
        public int RootAddress { get; set; }

        public static DiskProperties Load(BinaryReader reader) {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var dp = new DiskProperties();
            var buffer = new byte[28];
            reader.Read(buffer, 0, 28);

            dp.RootAddress = BitConverter.ToInt32(buffer, 0);
            dp.NumberOfBlocks = BitConverter.ToInt32(buffer, 4);
            dp.NumberOfUsedBlocks = BitConverter.ToInt32(buffer, 8);
            dp.MaximumSize = BitConverter.ToDouble(buffer, 12);
            dp.BlockSize = BitConverter.ToInt32(buffer, 20);
            var nameLength = BitConverter.ToInt32(buffer, 24);

            var nameBuffer = new byte[nameLength];
            reader.Read(nameBuffer, 0, nameLength);

            dp.Name = Encoding.ASCII.GetString(nameBuffer);
            
            return dp;
        }

        public static void Write(BinaryWriter writer, DiskProperties diskProperties) {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (diskProperties == null)
                throw new ArgumentNullException("diskProperties");
            writer.Write(diskProperties.NumberOfBlocks);
            writer.Write(diskProperties.NumberOfUsedBlocks);
            writer.Write(diskProperties.MaximumSize);
            writer.Write(diskProperties.BlockSize);
            if (diskProperties.Name.Length > 128)
            {
                diskProperties.Name = diskProperties.Name.Substring(0, 128);
            }
            writer.Write(diskProperties.Name.Length);
            writer.Write(diskProperties.Name.ToCharArray());
        }
    }
}