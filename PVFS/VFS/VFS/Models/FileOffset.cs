namespace VFS.VFS.Models
{
    static class FileOffset
    {
        public const int NextBlock = 0;
        /// <summary>
        /// Do not use this property anymore. Defrag not working otherwise
        /// </summary>
        public const int StartBlock = 4;
        public const int FileSize = 8;
        public const int NumberOfBlocks = 12;
        public const int IsDirectory = 16;
        public const int ParentAddress = 17;
        public const int NameLength = 21;
        public const int Name = 22;
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
