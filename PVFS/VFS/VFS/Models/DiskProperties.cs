﻿using System;
using System.IO;

namespace VFS.VFS.Models {
    public class DiskProperties {
        public string Name { get; set; }
        public int NumberOfBlocks { get; set; }
        public int NumberOfUsedBlocks { get; set; }
        public int MaximumSize { get; set; }

        public static DiskProperties Load(BinaryReader reader) {
            throw new NotImplementedException();
        }

        public static void Write(BinaryWriter path) {
            throw new NotImplementedException();
        }
    }
}