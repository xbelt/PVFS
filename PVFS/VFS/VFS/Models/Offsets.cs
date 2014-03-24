namespace VFS.VFS.Models
{
    static class Offsets
    {
        public static readonly int NextBlock = 0;
        public static readonly int StartBlock = 4;
        public static readonly int FileSize = 8;
        public static readonly int NumberOfBlocks = 12;
        public static readonly int IsDirectory = 16;
        public static readonly int NameLength = 17;
        public static readonly int Name = 18;
    }
}
