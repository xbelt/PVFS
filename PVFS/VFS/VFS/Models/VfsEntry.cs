using System;

namespace VFS.VFS.Models {
    public abstract class VfsEntry {
        public int Address { get; set; }
        public VfsFile Open(string path) {
            throw new NotImplementedException();
        }
    }
}