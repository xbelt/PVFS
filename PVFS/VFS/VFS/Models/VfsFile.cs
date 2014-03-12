using System.Collections.Generic;

namespace VFS.VFS.Models
{
    class VfsFile
    {
        public int Address { get; set; }
        public int FileSize { get; set; }
        public List<Block> Inodes { get; set; }
        public VfsDirectory Parent { get; set; }
        public string Name { get; set; }

        public string Type {
            get { return isDirectory ? null : Name.Substring(Name.LastIndexOf(".") + 1); }
        }

        public bool isDirectory { get; set; }
    }
}
