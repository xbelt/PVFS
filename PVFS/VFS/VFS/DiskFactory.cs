using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using VFS.VFS.Extensions;
using VFS.VFS.Models;
using VFS.VFS.Parser;

namespace VFS.VFS
{
    public class DiskFactory : Factory
    {
        //TODO: L store readers/writers in disk
        public static VfsDisk Create(DiskInfo info)
        {
            var disk = new VfsDisk(info.Path, new DiskProperties{BlockSize = info.BlockSize, MaximumSize = info.Size, Name = info.Name.Remove(info.Name.LastIndexOf(".")), NumberOfBlocks = (int)Math.Ceiling(info.Size/info.BlockSize), NumberOfUsedBlocks = 1});
            if (Directory.Exists(info.Path))
            {
                var writer = disk.getWriter();

                //blocksForPreamble + RootAddress + #Block + #UsedBlocks + Size + BlockSize + NameLength + Name + BitMap
                var numberOfUsedBitsInPreamble = disk.DiskProperties.NumberOfBlocks + (4 + 4 + 4 + 8 + 4 + 4 + 4*128) * 8;
                var blocksUsedForPreamble = (int)Math.Ceiling((double)numberOfUsedBitsInPreamble / (disk.DiskProperties.BlockSize*8));
                //Write disk info
                //writes the address of root
                writer.Write(blocksUsedForPreamble);
                disk.DiskProperties.RootAddress = blocksUsedForPreamble;

                disk.DiskProperties.NumberOfUsedBlocks = blocksUsedForPreamble + 1;
                
                DiskProperties.Write(writer, disk.DiskProperties);
                writer.Seek(disk, 0, disk.DiskProperties.BitMapOffset);
                //write bitMap
                for (var i = 0; i < Math.Ceiling((blocksUsedForPreamble + 1)/8d); i++)
                {
                    byte firstByte = 0;
                    for (var j = 0; j < (blocksUsedForPreamble + 1) - 8*i; j++)
                    {
                        firstByte += (byte)Math.Pow(2, 7 - j);
                        if (j == 7)
                        {
                            break;
                        }
                    }
                    writer.Write(firstByte);
                }

                byte one = 1;

                writer.Flush();

                //Write root folder manually
                writer.Seek(disk, blocksUsedForPreamble);
                writer.Write(0); //NextBlock
                writer.Write(blocksUsedForPreamble); //StartBlock
                writer.Write(0); //NrOfChildren
                writer.Write(1); //NoBlocks
                writer.Write(one); //Directory?
                writer.Write((byte)disk.DiskProperties.Name.Length); //NameSize
                writer.Write(disk.DiskProperties.Name.ToCharArray());
                
                writer.Flush();
                disk.Init();
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
                var reader = new BinaryReader(stream, new ASCIIEncoding(), false);
                var dp = DiskProperties.Load(reader);
                reader.Close();
                if (path.EndsWith(".vdi"))
                {
                    path = path.Remove(path.LastIndexOf("\\"));
                }
                var vfsDisk = new VfsDisk(path, dp);
                vfsDisk.Init();
                return vfsDisk;
            }
            throw new InvalidPathException(path + " is not a valid path to a vdi");
        }

        public static void Remove(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new DiskNotFoundException();
            }
        }
    }

    internal class InvalidPathException : Exception
    {
        public InvalidPathException(string s)
        {
            throw new NotImplementedException();
        }
    }

    public class DiskInfo {
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
