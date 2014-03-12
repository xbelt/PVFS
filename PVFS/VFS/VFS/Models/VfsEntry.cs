using System;

namespace VFS.VFS.Models {
    public abstract class VfsEntry {
        public int Address { get; set; }

        public bool isDirectory { get; set; }
        public static VfsFile Open() {
            throw new NotImplementedException();
        }
    }
}