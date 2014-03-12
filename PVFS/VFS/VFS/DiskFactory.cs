using System;
using VFS.VFS.Models;

namespace VFS.VFS
{
    class DiskFactory : Factory
    {
        public VfsDisk create(DiskInfo info) {
            throw new NotImplementedException();
        }
    }

    internal class DiskInfo {
        private String _path;
        private String _name;
        private int _size;
        private int _blockSize = 2048;

        public DiskInfo(string path, string name, int size, int blockSize) {
            _path = path;
            _name = name;
            _size = size;
            _blockSize = blockSize;
        }

        public string Path { get; set; }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public int Size {
            get { return _size; }
            set { _size = value; }
        }

        public int BlockSize {
            get { return _blockSize; }
            set { _blockSize = value; }
        }
    }
}
