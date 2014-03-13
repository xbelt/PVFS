using System;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class EntryFactory : Factory
    {
        /// <summary>
        /// creates a file on the disk with the specified size, with random content
        /// throws exceptions if the file exists/invalid size/invalid path/disk too full
        /// </summary>
        /// <returns>a handle for the file</returns>
        public VfsFile createFile(VfsDisk disk, string path, int size)
        {

            throw new NotImplementedException();
        }
        /// <summary>
        /// creates an empty directory on the disk
        /// throws exceptions if the directory exists/invalid path/disk too full
        /// </summary>
        /// <returns>a handle for the file</returns>
        public VfsFile createDirectory(VfsDisk disk, string path)
        {

            throw new NotImplementedException();
        }
    }

    internal class EntryInfo {
        
    }
}
