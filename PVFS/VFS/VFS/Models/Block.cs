using System.ComponentModel;

namespace VFS.VFS.Models {
    internal class Block {
        public int Address { get; set; }
        public int StartBlock { get; set; }
        public Block NextBlock { get; set; }
    }
}