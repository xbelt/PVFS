using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;

namespace UnitTest
{
    [TestClass]
    public class VfsDirectoryTest
    {

        [TestMethod]
        public void TestAddEntry()
        {
            string path, name;
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 103 * 200, 200);

            for (int i = 0; i < 100; i++)
            {
                VFSManager.CreateFile("/" + name + "/" + i);
            }

            VfsEntry e = EntryFactory.OpenEntry(disk, disk.root.Address, null);

            Debug.Assert(e != null);
            Debug.Assert(e.IsDirectory);
            Debug.Assert(e.Name == name);

            VfsDirectory dir = (VfsDirectory)e;

            Debug.Assert(dir.GetEntries().Count == 100);
            for (int i = 0; i < 100; i++)
            {
                Debug.Assert(dir.GetFile(""+i) != null);
            }
        }

        [TestMethod]
        public void TestRemoveEntry()
        {
            string path, name;
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 110 * 160, 160);

            for (int i = 0; i < 100; i++)
            {
                VFSManager.CreateFile("/" + name + "/" + i);
            }

            VFSManager.Remove("/"+name+"/33");

            VfsEntry e = EntryFactory.OpenEntry(disk, disk.root.Address, null);

            Debug.Assert(e != null);
            Debug.Assert(e.IsDirectory);
            Debug.Assert(e.Name == name);

            VfsDirectory dir = (VfsDirectory)e;

            Debug.Assert(dir.GetEntries().Count == 100);
            for (int i = 0; i < 100; i++)
            {
                if (i==33) continue;
                Debug.Assert(dir.GetFile(""+i) != null);
            }
        }

    }
}
