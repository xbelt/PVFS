namespace VFS.VFS.Models
{
    static class Offset
    {
        public const int NextBlock = 0;
        public const int StartBlock = 4;
        public const int FileSize = 8;
        public const int NumberOfBlocks = 12;
        public const int IsDirectory = 16;
        public const int NameLength = 17;
        public const int Name = 18;
    }
}
