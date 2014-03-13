using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class DiskFactory : Factory
    {
        public static VfsDisk Create(DiskInfo info)
        {
            VfsDisk disk = new VfsDisk(info.Path, new DiskProperties{BlockSize = info.BlockSize, MaximumSize = info.Size, Name = info.Name.Remove(info.Name.LastIndexOf(".")), NumberOfBlocks = (int)Math.Ceiling(info.Size/info.BlockSize), NumberOfUsedBlocks = 1});
            if (Directory.Exists(info.Path))
            {
                FileStream stream;
                if (info.Path.EndsWith("\\"))
                {
                    stream = File.Open(info.Path + info.Name, FileMode.Create, FileAccess.ReadWrite);
                }
                else
                {
                    stream = File.Open(info.Path + "\\" + info.Name, FileMode.Create, FileAccess.ReadWrite);
                }
                disk.FileStream = stream;
                var writer = new BinaryWriter(stream, new ASCIIEncoding(), true);

                //blocksForPreamble + RootAddress + #Block + #UsedBlocks + Size + BlockSize + NameLength + Name + BitMap
                var numberOfUsedBitsInPreamble = disk.DiskProperties.NumberOfBlocks + (4 + 4 + 4 + 8 + 4 + 4 + 4*128) * 8;
                var blocksUsedForPreamble = (int)Math.Ceiling((double)numberOfUsedBitsInPreamble / (disk.DiskProperties.BlockSize*8));
                //Write disk info
                //writes the address of root
                writer.Write(blocksUsedForPreamble);
                
                DiskProperties.Write(writer, disk.DiskProperties);

                //write bitMap
                for (int i = 0; i < Math.Ceiling((blocksUsedForPreamble + 1)/8d); i++)
                {
                    byte firstByte = 0;
                    for (int j = 0; j < (blocksUsedForPreamble + 1) - 8*i; j++)
                    {
                        firstByte += (byte)Math.Pow(2, 7 - j);
                        if (j == 7)
                        {
                            break;
                        }
                    }
                    writer.Write(firstByte);
                }

                byte zero = 0;
                byte one = 1;

                for (int i = 1; i < Math.Ceiling(disk.DiskProperties.NumberOfBlocks/4d); i++)
                {
                    writer.Write(zero);
                }
                writer.Flush();

                //Write root folder manually
                writer.Write(blocksUsedForPreamble); //NextBlock
                writer.Write(blocksUsedForPreamble); //StartBlock
                writer.Write(0); //NrOfChildren
                writer.Write(1); //NoBlocks
                writer.Write(one); //Directory?
                writer.Write(disk.DiskProperties.Name.Length); //NameSize
                writer.Write(disk.DiskProperties.Name);
                for (int i = 0; i < disk.DiskProperties.BlockSize - 18 - disk.DiskProperties.Name.Length - 4; i++)
                {
                    writer.Write(zero); //Fill block with 0's
                }
                writer.Close();
            }
            else
            {
                throw new InvalidPathException(info + " is not a valid path");
            }
            return disk;
        }

        public static VfsDisk Load(string path)
        {
            if (File.Exists(path))
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                var reader = new BinaryReader(stream, new ASCIIEncoding(), true);
                var dp = DiskProperties.Load(reader);
                return new VfsDisk(path, dp);
            }
            throw new InvalidPathException(path + " is not a valid path to a vdi");
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
