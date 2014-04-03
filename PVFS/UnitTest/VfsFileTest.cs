using System;
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
            VfsManager.AddAndOpenDisk(disk);
            return disk;
        }

        private void end(string name)
        {
            VfsManager.UnloadDisk(name);
        }

        [TestMethod]
        public void GettersTest()
        {
            string name;
            VfsDisk disk = setup(out name);

            VfsFile cfile = EntryFactory.createFile(disk, "a", 1000, disk.Root);
            VfsFile file = (VfsFile)EntryFactory.OpenEntry(disk, cfile.Address, disk.Root);

            Debug.Assert(VfsFile.GetNoBlocks(disk, 1000) == 6);
            Debug.Assert(!file.ToString().Contains("(loaded)"));
            Debug.Assert(file.GetAbsolutePath() == "/" + name + "/a");
            PrivateObject o = new PrivateObject(file);
            o.Invoke("Load", new object[]{ });
            Debug.Assert(file.ToString().Contains("(loaded)"));


            end(name);
        }

        [TestMethod]
        public void TestWrite()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            Assert.AreNotSame(null, disk);
            VfsManager.AddAndOpenDisk(disk);
            var file = VfsManager.CreateFile("/" + name + "/fs.txt");
            Assert.AreNotSame(null, file);
            var fileSize1 = file.FileSize;
            Assert.AreNotSame(null, fileSize1);
            Directory.CreateDirectory("C:\\Test");
            var writer1 = File.CreateText("C:\\Test\\myText.txt");
            writer1.Write("I'm in Test.");
            writer1.Close();
            var stream = File.Open("C:\\Test\\myText.txt", FileMode.Open, FileAccess.ReadWrite);
            Assert.AreNotSame(null, stream);
            var reader = new BinaryReader(stream);
            Assert.AreNotSame(null, reader);
            file.Write(reader);

            var fileSize2 = file.FileSize;
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
            VfsManager.AddAndOpenDisk(disk);
            var file = VfsManager.CreateFile("/" + name + "/fs.txt");
            Assert.AreNotSame(null, disk);
            Assert.AreNotSame(null, disk.Root);
            Assert.AreNotSame(null, disk.Root.GetFile("fs.txt"));
            Assert.AreEqual("txt", disk.Root.GetFile("fs.txt").FileType);
        }
    
        [TestMethod]
        public void RenameTest()
        {
            string name;
            VfsDisk disk = setup(out name);

            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.Root);



            end(name);
        }
        [TestMethod]
        public void ReadWriteTest()
        {
            string name;
            VfsDisk disk = setup(out name);
            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.Root);
            end(name);
        }
        [TestMethod]
        public void FreeTest()
        {
            string name;
            VfsDisk disk = setup(out name);
            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.Root);
            end(name);
        }
        [TestMethod]
        public void DuplicateTest()
        {
            string name;
            VfsDisk disk = setup(out name);
            VfsFile file = EntryFactory.createFile(disk, "a", 1000, disk.Root);
            end(name);
        }
    }
}
