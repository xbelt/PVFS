using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;

namespace UnitTest
{
    [TestClass]
    public class VFSManagerTests
    {
        [TestMethod]
        public void TestAddAndOpenDisk()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            Debug.Assert(VfsManager.CurrentDisk == disk);
            Debug.Assert(VfsManager.WorkingDirectory == disk.Root);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestGetDisk()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);

            var accessor = new PrivateType(typeof (VfsManager));
            accessor.InvokeStatic("AddAndOpenDisk", new [] {disk});
            var result = accessor.InvokeStatic("getDisk", new[] {name});
            Debug.Assert(result == disk);
            disk.Stream.Close();
        }

        //----------------------Working Directory----------------------

        [TestMethod]
        public void TestChangeWorkingDirectory()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/"+name+"/b/c", true);
            VfsManager.ChangeWorkingDirectory("/"+name+"/b/c");
            Assert.AreEqual(VfsManager.WorkingDirectory.GetAbsolutePath(), "/" + name + "/b/c");
        }


        //--------------------------------------------------------------


        [TestMethod]
        public void TestGetEntryPathOnlineFile()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            var file = VfsManager.CreateFile("/" + name + "/testFile");
            var readFile = VfsManager.GetEntry("/" + name + "/testFile");
            Debug.Assert(file == readFile);
        }

        [TestMethod]
        public void TestGetEntryPathOnlineFolder()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VfsManager.GetEntry("/" + name + "/testFolder");
            var directoryFile = readFile as VfsDirectory;
            Debug.Assert(readFile.IsDirectory);
            Debug.Assert(readFile.Address == 2);
            Debug.Assert(readFile.Name == "testFolder");
            Debug.Assert(directoryFile.Parent.Address
                 == 1);

        }

        [TestMethod]
        public void TestGetEntryPathOfflineFile()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            var file = VfsManager.CreateFile("/" + name + "/testFile");
            VfsManager.UnloadDisk(name);
            disk = DiskFactory.Load(path, null);
            VfsManager.AddAndOpenDisk(disk);
            var readFile = VfsManager.GetEntry("/" + name + "/testFile");
            var newFile = readFile as VfsFile;
            Debug.Assert(readFile != null);
            Debug.Assert(file.Parent.Address == newFile.Parent.Address);
            Debug.Assert(file.Address == newFile.Address);
            Debug.Assert(file.IsDirectory == newFile.IsDirectory);
            Debug.Assert(file.Name == newFile.Name);
        }

        [TestMethod]
        public void TestGetEntryPathOfflineFolder()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testDir", true);
            var file = VfsManager.GetEntry("/" + name + "/testDir") as VfsDirectory;
            VfsManager.UnloadDisk(name);
            disk = DiskFactory.Load(path, null);
            VfsManager.AddAndOpenDisk(disk);
            var readFile = VfsManager.GetEntry("/" + name + "/testDir");
            var newDirectory = readFile as VfsDirectory;
            Debug.Assert(readFile != null);
            Debug.Assert(file.Parent.Address == newDirectory.Parent.Address);
            Debug.Assert(file.Address == newDirectory.Address);
            Debug.Assert(file.IsDirectory == newDirectory.IsDirectory);
            Debug.Assert(file.Name == newDirectory.Name);
        }

        [TestMethod]
        public void TestGetAbsolutePath()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            Debug.Assert(VfsManager.GetAbsolutePath("") == "/"+name);
            Debug.Assert(VfsManager.GetAbsolutePath(".") == "/"+name);
            Debug.Assert(VfsManager.GetAbsolutePath("..") == "/"+name);
            Debug.Assert(VfsManager.GetAbsolutePath("/"+name) == "/"+name);
            VfsManager.CreateDirectory("/"+name+"/a", true);
            Debug.Assert(VfsManager.GetAbsolutePath("a") == "/"+name+"/a");
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestUnloadDisk()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);

            var accessor = new PrivateType(typeof(VfsManager));
            accessor.InvokeStatic("AddAndOpenDisk", new[] { disk });
            accessor.InvokeStatic("UnloadDisk", new[] { name });
            var result = accessor.InvokeStatic("getDisk", new[] { name });
            Debug.Assert(result == null);
        }

        [TestMethod]
        public void TestGetTempFilePath() {
            var accessor = new PrivateType(typeof(VfsManager));
            var result = accessor.InvokeStatic("getTempFilePath", new object[] {});
        }

        [TestMethod]
        public void TestChangeDirectoryByIdentifier() {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/b/c", true);
            VfsManager.ChangeDirectoryByIdentifier("b/c");
            Assert.AreEqual(VfsManager.WorkingDirectory.GetAbsolutePath(), "/" + name + "/b/c");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetEntryArgNullException() {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VfsManager.GetEntry(disk, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetEntryArgException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VfsManager.GetEntry(disk, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetAbsolutePathArgNullException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VfsManager.GetAbsolutePath(null);
        }

        [TestMethod]
        public void TestRemoveDirectory()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            VfsManager.CreateDirectory("/" + name + "/testFolder/b", true);
            VfsManager.CreateFile("/" + name + "/testFolder/a");
            VfsManager.Remove("/" + name + "/testFolder");
        }

        [TestMethod]
        public void TestRemoveDirectoryByIdent()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            VfsManager.CreateDirectory("/" + name + "/testFolder/b", true);
            VfsManager.CreateFile("/" + name + "/testFolder/a");
            VfsManager.RemoveByIdentifier("testFolder");
        }

        [TestMethod]
        public void TestDefrag()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateFile("/" + name + "/a");
            VfsManager.CreateFile("/" + name + "/b");
            VfsManager.CreateFile("/" + name + "/c");
            VfsManager.Remove("/" + name + "/a");
            VfsManager.Defrag();
        }

        [TestMethod]
        public void TestGetFreeSpace()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 900, 300);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.GetFreeSpace();
            VfsManager.GetOccupiedSpace();
        }

        [TestMethod]
        public void TestExit()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 900, 300);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.Exit();
        }

        [TestMethod]
        public void TestMove()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VfsManager.AddAndOpenDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            VfsManager.CreateFile("/" + name + "/a");
            VfsManager.Move("/" + name + "/a", "/" + name + "/testFolder");
            var directory = VfsManager.GetEntry("/" + name + "/testFolder") as VfsDirectory;
            var file = VfsManager.GetEntry("/" + name + "/testFolder/a") as VfsFile;
            Debug.Assert(file.Parent.Address == directory.Address);
        }
    }
}
