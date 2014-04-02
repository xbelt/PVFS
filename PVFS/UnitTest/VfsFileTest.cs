using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;

namespace UnitTest
{
    [TestClass]
    public class VfsFileTest
    {
        private VfsDisk setup(out string name)
        {
            string path;
            VfsDisk disk = DiskFactoryTests.createTestDisk(out path, out name, 200 * 100, 200);
            VFSManager.AddAndOpenDisk(disk);
            return disk;
        }

        private void end(string name)
        {
            VFSManager.UnloadDisk(name);
        }

        [TestMethod]
        public void GettersTest()
        {
            string name;
            VfsDisk disk = setup(out name);

            VfsFile cfile = EntryFactory.createFile(disk, "a", 1000, disk.root);
            VfsFile file = (VfsFile)EntryFactory.OpenEntry(disk, cfile.Address, disk.root);

            Debug.Assert(VfsFile.GetNoBlocks(disk, 1000) == 6);
            Debug.Assert(!file.ToString().Contains("(loaded)"));
            Debug.Assert(file.GetAbsolutePath() == "/" + name + "/a");
            PrivateObject o = new PrivateObject(file);
            o.Invoke("Load", new { });
            Debug.Assert(file.ToString().Contains("(loaded)"));


            end(name);
        }

        [TestMethod]
        public void TestgetType()
        {   //You have to have a local disk with name b and a file fs.txt in root directory
            const string path = "C:\\Test\\b.vdi";
            var disk = DiskFactory.Load(path, "a");
            Assert.AreNotSame(null, disk);
            Assert.AreNotSame(null, disk.root);
            Assert.AreNotSame(null, disk.root.GetFile("fs.txt"));
            Assert.AreEqual("txt", disk.root.GetFile("fs.txt").Type);
        }
    
        [TestMethod]
        public void RenameTest()
        {
            string name;
            VfsDisk disk = setup(out name);

            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.root);



            end(name);
        }
        [TestMethod]
        public void ReadWriteTest()
        {
            string name;
            VfsDisk disk = setup(out name);

            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.root);



            end(name);
        }
        [TestMethod]
        public void FreeTest()
        {
            string name;
            VfsDisk disk = setup(out name);

            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.root);



            end(name);
        }
        [TestMethod]
        public void DuplicateTest()
        {
            string name;
            VfsDisk disk = setup(out name);

            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.root);



            end(name);
        }
    }
}
