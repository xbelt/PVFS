using System;

namespace VFS.VFS.Models {
    public abstract class VfsEntry {
        public int Address { get; set; }
        public virtual void Open(string path) {
            throw new NotImplementedException();
        }

        public virtual void Open(int address)
        {
            throw new NotImplementedException();
        }
    }
}