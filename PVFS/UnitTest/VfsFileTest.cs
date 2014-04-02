using System.Diagnostics;
using System.IO;
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
        public void TestWrite()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            Assert.AreNotSame(null, disk);
            VFSManager.AddAndOpenDisk(disk);
            var file = VFSManager.CreateFile("/" + name + "/fs.txt");
            Assert.AreNotSame(null, file);
            var fileSize1 = disk.DiskProperties.NumberOfUsedBlocks;
            Assert.AreNotSame(null, fileSize1);
            var fileInfo = new FileInfo("C:\\Test\\myText.txt");
            Assert.AreNotSame(null, fileInfo);
            var reader = new BinaryReader(fileInfo.OpenRead());
            Assert.AreNotSame(null, reader);
            file.Write(reader);

            var fileSize2 = disk.DiskProperties.NumberOfUsedBlocks;
            Assert.AreNotSame(null, fileSize2);
            reader.Dispose();
            reader.Close();
            Assert.AreNotEqual(fileSize1, fileSize2);
        }

        [TestMethod]
        public void TestgetType()
        {   //You have to have a local disk with name b and a file fs.txt in root directory
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            var file = VFSManager.CreateFile("/" + name + "/fs.txt");
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
