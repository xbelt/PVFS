using System.Diagnostics;
using System.Linq;
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
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 106 * 200, 200);
            VFSManager.AddAndOpenDisk(disk);

            for (int i = 0; i < 100; i++)
            {
                VFSManager.CreateFile("/" + name + "/" + i);
            }

            VfsEntry e = EntryFactory.OpenEntry(disk, disk.Root.Address, null);

            Debug.Assert(e != null);
            Debug.Assert(e.IsDirectory);
            Debug.Assert(e.Name == name);

            VfsDirectory dir = (VfsDirectory)e;

            Debug.Assert(dir.GetEntries().ToList().Count == 100);
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
            VFSManager.AddAndOpenDisk(disk);

            for (int i = 0; i < 100; i++)
            {
                VFSManager.CreateFile("/" + name + "/" + i);
            }
            for (int i = 0; i < 41; i++)
            {
                VFSManager.Remove("/" + name + "/" + (33 + i));
            }

            VfsEntry e = EntryFactory.OpenEntry(disk, disk.Root.Address, null);

            Debug.Assert(e != null);
            Debug.Assert(e.IsDirectory);
            Debug.Assert(e.Name == name);

            VfsDirectory dir = (VfsDirectory)e;

            Debug.Assert(dir.GetEntries().ToList().Count == 59);
            for (int i = 0; i < 100; i++)
            {
                if (i>=33&& i<74) continue;
                Debug.Assert(dir.GetFile(""+i) != null);
            }
        }

        [TestMethod]
        public void TestGetters()
        {
            string path, name;
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 110 * 160, 160);
            VFSManager.AddAndOpenDisk(disk);

            disk.Root.GetEntry("");
            disk.Root.GetFile("");
            disk.Root.GetEntry("");
            disk.Root.ToString();
        }

        [TestMethod]
        public void TestGetDirectory()
        {
            string path, name;
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 110 * 160, 160);
            VFSManager.AddAndOpenDisk(disk);

            VFSManager.CreateDirectory("/" + name + "/a", true);
            VFSManager.CreateDirectory("/" + name + "/b", true);

            var dir = VFSManager.workingDirectory.GetDirectory("a");
            Assert.AreEqual(dir.Name, "a");

            var dir2 = VFSManager.workingDirectory.GetDirectory("c");
            Assert.AreEqual(null, dir2);
        }

        [TestMethod]
        public void TestGetFiles()
        {
            string path, name;
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 110 * 160, 160);
            VFSManager.AddAndOpenDisk(disk);

            VFSManager.CreateFile("/" + name + "/a");
            VFSManager.CreateFile("/" + name + "/b");

            var list = VFSManager.workingDirectory.GetFiles().ToList();
            Assert.AreEqual(list.Count, 2);
            Assert.AreEqual(list.Exists(x => x.Name == "a"), true);
        }
    }
}
