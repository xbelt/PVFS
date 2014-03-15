using System;
using System.IO;

namespace VFS.VFS.Models {
    /// <summary>
    /// This class should be used as returntype if you can return both, a VfsFile or a VfsDirectory
    /// </summary>
    public abstract class VfsEntry {
        public int Address { get; protected set; }
        /// <summary>
        /// Indicates whether this is a VfsFile or VfsDirectory
        /// </summary>
        public bool IsDirectory { get; protected set; }
    }
}