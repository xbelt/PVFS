using System;
using System.IO;

namespace VFS.VFS.Models {
    public class DiskProperties {
        public string Name { get; set; }
        public int NumberOfBlocks { get; set; }
        public int NumberOfUsedBlocks { get; set; }
        public double MaximumSize { get; set; }
        public int BlockSize { get; set; }
        public int RootAddress { get; set; }

        public static DiskProperties Load(BinaryReader reader) {
            var dp = new DiskProperties();
            var buffer = new byte[28];
            reader.Read(buffer, 0, 28);
            dp.RootAddress = BitConverter.ToInt32(buffer, 0);
        }

        public static void Write(BinaryWriter path) {
            throw new NotImplementedException();
        }
    }
}