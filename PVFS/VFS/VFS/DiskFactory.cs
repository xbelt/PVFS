using System;
using System.IO;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class DiskFactory : Factory
    {
        public VfsDisk create(DiskInfo info)
        {
            VfsDisk disk = new VfsDisk(info.Path, new DiskProperties{BlockSize = info.BlockSize, MaximumSize = info.Size, Name = info.Name, NumberOfBlocks = (int)Math.Ceiling(info.Size/info.BlockSize), NumberOfUsedBlocks = 1});
            if (Directory.Exists(info.Path))
            {
                var stream = File.Open(info.Path + "\\" + info.Name, FileMode.Create, FileAccess.ReadWrite);
                disk.FileStream = stream;
                var writer = new BinaryWriter(stream);

                //BitMap + RootAddress + #Block + #UsedBlocks + Size + NameLength + Name
                var numberOfUsedBitsInPreamble = disk.DiskProperties.NumberOfBlocks + 4 + 4 + 4 + 8 + 4 + 4*128;
                var blocksUsedForPreamble = (int)Math.Ceiling((double)numberOfUsedBitsInPreamble / (disk.DiskProperties.BlockSize*8));
                //write bitMap
                byte firstByte = 0;
                for (int i = 0; i <= blocksUsedForPreamble; i++)
                {
                    firstByte += (byte) Math.Pow(2, 7 - i);
                }
                byte zero = 0;
                writer.Write(firstByte);

                for (int i = 1; i < Math.Ceiling(disk.DiskProperties.NumberOfBlocks/4d); i++)
                {
                    writer.Write(zero);
                }
                writer.Flush();
                //Write root address
                writer.Write(blocksUsedForPreamble);
                writer.Write(disk.DiskProperties.NumberOfBlocks);
                writer.Write(disk.DiskProperties.NumberOfUsedBlocks);
                writer.Write(disk.DiskProperties.MaximumSize);
                if (disk.DiskProperties.Name.Length > 128)
                {
                    disk.DiskProperties.Name = disk.DiskProperties.Name.Substring(0, 128);
                }
                writer.Write(disk.DiskProperties.Name.Length);
                writer.Write(disk.DiskProperties.Name.ToCharArray());
                writer.Flush();
            }
            else
            {
                throw new InvalidPathException(info + " is not a valid path");
            }
            return disk;
        }
    }

    internal class InvalidPathException : Exception
    {
        public InvalidPathException(string s)
        {
            throw new NotImplementedException();
        }
    }

    internal class DiskInfo {
        private String _path;
        private String _name;
        private double _size;
        private int _blockSize = 2048;

        public DiskInfo(string path, string name, double size, int blockSize) {
            Path = path;
            Name = name;
            Size = size;
            BlockSize = blockSize;
        }

        public string Path { get; set; }

        public string Name { get; set; }

        public double Size { get; set; }

        public int BlockSize {
            get { return _blockSize; }
            set { _blockSize = value; }
        }
    }
}
