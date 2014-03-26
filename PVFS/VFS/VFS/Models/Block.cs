using System.ComponentModel;

namespace VFS.VFS.Models {
    public class Block {
        public Block(int address, Block nextBlock)
        {
            Address = address;
            NextBlock = nextBlock;
        }

        public int Address { get; set; }
        public Block NextBlock { get; set; }
    }
}