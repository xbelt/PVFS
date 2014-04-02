using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFS.VFS;

namespace UnitTest
{
    class VfsDirectoryTest_
    {
        public static string wDir = Directory.GetCurrentDirectory();

        public static string PrepareDisk(int blocksize, int size)
        {
            DiskFactory.Create(new DiskInfo(wDir, "dirTest", size, blocksize), null);
            return "";
        }

        public void TestAddEntry()
        {
            DiskFactory.Create(new DiskInfo(wDir, "dirTest", 1024 * 100, 1024), null);

        }

    }
}
