using System.Collections.Generic;
using System.IO;

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

        public void Write(BinaryReader reader) {
            
        }

        public void Read(BinaryWriter writer) {
            
        }
    }
}
