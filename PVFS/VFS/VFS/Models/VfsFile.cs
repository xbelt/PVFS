using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VFS.VFS.Models
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
    public class VfsFile : VfsEntry
    {
        public List<Block> Inodes { get; set; }
        public bool isDirectory { get; set; }
        public VfsDirectory Parent { get; set; }
        public string Name { get; set; }
        public string Type {
            get { return isDirectory ? null : Name.Substring(Name.LastIndexOf(".") + 1); }
        }

        private VfsDisk disk;

        public VfsFile(VfsDisk disk)
        {
            this.disk = disk;
        }

        public void Read(BinaryReader reader) {
            
        }

        public void Write(BinaryWriter writer) {
            
        }

        public override void Open(string path)
        {
            base.Open(path);
        }

        public override void Open(int address)
        {
            //TODO: I am not yet sure how to get the parent directory
            var reader = new BinaryReader(disk.FileStream);
            var blockSize = disk.DiskProperties.BlockSize;
            var buffer = new Byte[blockSize];
            reader.Read(buffer, address*blockSize, blockSize);
            var numberOfBlocks = BitConverter.ToInt32(buffer, 12);
            isDirectory = BitConverter.ToBoolean(buffer, 16);
            Name = BitConverter.ToString(buffer, 18, 110);
            Inodes.Add(new Block(address, address, new Block(address, address, null)));
            var nextAddress = BitConverter.ToInt32(buffer, 0);

            for (int i = 0; i < numberOfBlocks - 1; i++)
            {
                reader.Read(buffer, nextAddress*blockSize, blockSize);
                var nextBlock = new Block(nextAddress, address, null);
                Inodes.Last().NextBlock = nextBlock;
                Inodes.Add(nextBlock);
                nextAddress = BitConverter.ToInt32(buffer, 0);
            }
        }
    }
}
