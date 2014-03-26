namespace VFS.VFS.Models
{
    static class FileOffset
    {
        public const int NextBlock = 0;
        public const int FileSize = 4;
        public const int NumberOfBlocks = 8;
        public const int IsDirectory = 12;
        public const int ParentAddress = 13;
        public const int NameLength = 17;
        public const int Name = 18;
        public const int Header = 128;
        public const int SmallHeader = 8;
    }

    static class DiskOffset
    {
        public const int RootAddress = 0;
        public const int NumberOfBlocks = 4;
        public const int NumberOfUsedBlocks = 8;
        public const int MaximumSize = 12;
        public const int BlockSize = 20;
        public const int NameLength = 24;
        public const int Name = 28;
    }
}
