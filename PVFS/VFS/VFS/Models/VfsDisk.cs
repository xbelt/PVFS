using System;
using System.IO;

namespace VFS.VFS.Models
{
    class VfsDisk {

        public VfsDisk(string path, DiskProperties properties) {
            Path = path;
            DiskProperties = properties;
            //BitMap = BitMap.Load();
        }

        private BitMap BitMap { get; set; }
        private DiskProperties DiskProperties { get; set; }
        private string Path { get; set; }

        public bool isFull() {
            return DiskProperties.NumberOfBlocks == DiskProperties.NumberOfUsedBlocks;
        }

        public VfsDirectory root() {
            throw new NotImplementedException();
        }
        #region Block
        public bool allocate(out int address)
        {
            throw new NotImplementedException();
        }

        public bool allocate(out int[] address, int numberOfBlocks)
        {
            throw new NotImplementedException();
        }

        public void free(int address) 
        {
            throw new NotImplementedException();
        }
        #endregion


    }

    internal class BitMap {
        private BitMap Load(BinaryReader path) {
            throw new NotImplementedException();
        }

        private void Write(BinaryWriter path) {
            
        }

        private void Update(BinaryWriter path, int address, bool occupied) {
            
        }
    }
}
