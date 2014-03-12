using System.Collections.Generic;

namespace VFS.VFS.Models
{
    class VfsDirectory : VfsFile
    {
        public List<Block> Children { get; set; } 
    }
}
