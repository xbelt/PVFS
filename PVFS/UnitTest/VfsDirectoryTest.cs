using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;

namespace UnitTest
{
    [TestClass]
    class VfsDirectoryTest
    {
     
        [TestMethod]
        public void TestAddEntry()
        {
            string path, name;
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 103*200, 200);

            for (int i = 0; i < 100; i++)
            {
                VFSManager.CreateFile("/" + name + "/" + i);
            }

            VfsEntry e = EntryFactory.OpenEntry(disk, disk.root.Address, null);

            Debug.Assert(e != null);
            Debug.Assert(e.IsDirectory);
            Debug.Assert(e.Name == name);

            VfsDirectory dir = (VfsDirectory) e;

            Debug.Assert(dir.GetEntries().Count == 100);
            for (int i = 0; i < 100; i++)
            {
                Debug.Assert(dir.GetFile(i.ToString())!=null);
            }
        }

    }
}
