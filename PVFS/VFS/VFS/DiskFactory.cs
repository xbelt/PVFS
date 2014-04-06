using System;
using System.IO;
using System.Text;
using VFS.VFS.Extensions;
using VFS.VFS.Models;
using VFS.VFS.Parser;

namespace VFS.VFS
{
    public class DiskFactory : Factory
    {
        public static VfsDisk Create(DiskInfo info, string pw)
        {
            if (info == null)
            {
                return null;
            }
            var disk = new VfsDisk(info.Path, new DiskProperties{BlockSize = info.BlockSize, MaximumSize = info.Size, Name = info.Name.Remove(info.Name.LastIndexOf(".")), NumberOfBlocks = (int)Math.Ceiling(info.Size/info.BlockSize), NumberOfUsedBlocks = 1}, pw);
            var writer = disk.GetWriter;

            //blocksForPreamble + RootAddress + #Block + #UsedBlocks + Size + BlockSize + NameLength + Name + BitMap
            var numberOfUsedBitsInPreamble = disk.DiskProperties.NumberOfBlocks + (4 + 4 + 4 + 8 + 4 + 4 + 128) * 8;
            var blocksUsedForPreamble = (int)Math.Ceiling((double)numberOfUsedBitsInPreamble / (disk.DiskProperties.BlockSize*8));
            //Write disk info
            //writes the address of root
            writer.Write(blocksUsedForPreamble);
            disk.DiskProperties.RootAddress = blocksUsedForPreamble;

            disk.DiskProperties.NumberOfUsedBlocks = blocksUsedForPreamble + 1;
                
            DiskProperties.Write(writer, disk.DiskProperties);
            writer.Seek(disk, 0, DiskProperties.BitMapOffset);
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
            writer.Write(0L); //NrOfChildren
            writer.Write(1); //NoBlocks
            writer.Write(one); //Directory?
            writer.Write(blocksUsedForPreamble);
            writer.Write((byte)disk.DiskProperties.Name.Length); //NameSize
            writer.Write(disk.DiskProperties.Name.ToCharArray());
                
            writer.Flush();
            disk.Init();
            return disk;
        }

        public static VfsDisk Load(string path, string pw)
        {
            if (File.Exists(path))
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                var reader = new BinaryReader(stream, new ASCIIEncoding(), false);
                var dp = DiskProperties.Load(reader);
                reader.Close();
                if (path == null)
                    return null;
                if (path.EndsWith(".vdi"))
                {
                    path = path.Remove(path.LastIndexOf("\\"));
                }
                var vfsDisk = new VfsDisk(path, dp, pw);
                vfsDisk.Init();
                return vfsDisk;
            }
            throw new ArgumentException(path + " is not a valid path to a vdi");
        }

        public static void Remove(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new ArgumentException("path does not exist", "path");
            }
        }
    }

    public class DiskInfo {
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
