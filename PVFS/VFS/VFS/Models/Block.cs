using System.ComponentModel;

namespace VFS.VFS.Models {
    public class Block {
        public Block(int address, int startBlock, Block nextBlock)
        {
            Address = address;
            StartBlock = startBlock;
            NextBlock = nextBlock;
        }

        public int Address { get; set; }
        public int StartBlock { get; set; }
        public Block NextBlock { get; set; }
    }
}