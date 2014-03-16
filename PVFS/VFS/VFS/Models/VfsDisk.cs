using System;
using System.IO;

namespace VFS.VFS.Models
{
    public class VfsDisk {

        public VfsDisk(string path, DiskProperties properties) {
            Path = path;
            DiskProperties = properties;
            //BitMap = BitMap.Load();
        }

        public BinaryReader Reader;
        public BinaryWriter Writer;
        private BitMap BitMap { get; set; }
        public DiskProperties DiskProperties { get; set; }
        private string Path { get; set; }

        public Stream FileStream { get; set; }

        public BinaryReader getReader() 
        {
            return Reader;
        }
        public BinaryWriter getWriter()
        {
            return Writer;
        } 

        public bool isFull() {
            return DiskProperties.NumberOfBlocks == DiskProperties.NumberOfUsedBlocks;
        }

        public VfsDirectory root() { //Which root? Of the host-system?
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



        public int BlockSize
        {
            get { return this.DiskProperties.BlockSize; }
        }
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
