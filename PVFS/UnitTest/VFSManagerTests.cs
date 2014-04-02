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
            VFSManager.AddAndOpenDisk(disk);
            Debug.Assert(VFSManager.CurrentDisk == disk);
            Debug.Assert(VFSManager.workingDirectory == disk.Root);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestGetDisk()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);

            var accessor = new PrivateType(typeof (VFSManager));
            accessor.InvokeStatic("AddAndOpenDisk", new [] {disk});
            var result = accessor.InvokeStatic("getDisk", new[] {name});
            Debug.Assert(result == disk);
            disk.Stream.Close();
        }

        //----------------------Working Directory----------------------

        [TestMethod]
        public void TestChangeWorkingDirectory()
        { //TODO: get this to work
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/"+name+"/b/c", true);
            VFSManager.ChangeWorkingDirectory("/"+name+"/b/c");
            Assert.AreEqual(VFSManager.workingDirectory.GetAbsolutePath(), "/" + name + "/b/c");
        }


        //--------------------------------------------------------------


        [TestMethod]
        public void TestGetEntryPathOnlineFile()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            var file = VFSManager.CreateFile("/" + name + "/testFile");
            var readFile = VFSManager.getEntry("/" + name + "/testFile");
            Debug.Assert(file == readFile);
        }

        [TestMethod]
        public void TestGetEntryPathOnlineFolder()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VFSManager.getEntry("/" + name + "/testFolder");
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
            VFSManager.AddAndOpenDisk(disk);
            var file = VFSManager.CreateFile("/" + name + "/testFile");
            VFSManager.UnloadDisk(name);
            disk = DiskFactory.Load(path, null);
            VFSManager.AddAndOpenDisk(disk);
            var readFile = VFSManager.getEntry("/" + name + "/testFile");
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
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testDir", true);
            var file = VFSManager.getEntry("/" + name + "/testDir") as VfsDirectory;
            VFSManager.UnloadDisk(name);
            disk = DiskFactory.Load(path, null);
            VFSManager.AddAndOpenDisk(disk);
            var readFile = VFSManager.getEntry("/" + name + "/testDir");
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
            VFSManager.AddAndOpenDisk(disk);
            Debug.Assert(VFSManager.getAbsolutePath("") == "/"+name);
            Debug.Assert(VFSManager.getAbsolutePath(".") == "/"+name);
            Debug.Assert(VFSManager.getAbsolutePath("..") == "/"+name);
            Debug.Assert(VFSManager.getAbsolutePath("/"+name) == "/"+name);
            VFSManager.CreateDirectory("/"+name+"/a", true);
            Debug.Assert(VFSManager.getAbsolutePath("a") == "/"+name+"/a");
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestUnloadDisk()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);

            var accessor = new PrivateType(typeof(VFSManager));
            accessor.InvokeStatic("AddAndOpenDisk", new[] { disk });
            accessor.InvokeStatic("UnloadDisk", new[] { name });
            var result = accessor.InvokeStatic("getDisk", new[] { name });
            Debug.Assert(result == null);
        }

        [TestMethod]
        public void TestGetTempFilePath() {
            var accessor = new PrivateType(typeof(VFSManager));
            var result = accessor.InvokeStatic("getTempFilePath", new object[] {});
        }

        [TestMethod]
        public void TestChangeDirectoryByIdentifier() {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/b/c", true);
            VFSManager.ChangeDirectoryByIdentifier("b/c");
            Assert.AreEqual(VFSManager.workingDirectory.GetAbsolutePath(), "/" + name + "/b/c");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetEntryArgNullException() {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VFSManager.getEntry(disk, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetEntryArgException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VFSManager.getEntry(disk, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetAbsolutePathArgNullException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VFSManager.getAbsolutePath(null);
        }

        [TestMethod]
        public void TestRemoveDirectory()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testFolder", true);
            VFSManager.CreateDirectory("/" + name + "/testFolder/b", true);
            VFSManager.CreateFile("/" + name + "/testFolder/a");
            VFSManager.Remove("/" + name + "/testFolder");
        }

        [TestMethod]
        public void TestRemoveDirectoryByIdent()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testFolder", true);
            VFSManager.CreateDirectory("/" + name + "/testFolder/b", true);
            VFSManager.CreateFile("/" + name + "/testFolder/a");
            VFSManager.RemoveByIdentifier("testFolder");
        }

        [TestMethod]
        public void TestDefrag()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateFile("/" + name + "/a");
            VFSManager.CreateFile("/" + name + "/b");
            VFSManager.CreateFile("/" + name + "/c");
            VFSManager.Remove("/" + name + "/a");
            VFSManager.Defrag();
        }

        [TestMethod]
        public void TestGetFreeSpace()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 900, 300);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.GetFreeSpace();
            VFSManager.GetOccupiedSpace();
        }

        [TestMethod]
        public void TestExit()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 900, 300);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.Exit();
        }

        [TestMethod]
        public void TestMove()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VFSManager.AddAndOpenDisk(disk);
            VFSManager.CreateDirectory("/" + name + "/testFolder", true);
            VFSManager.CreateFile("/" + name + "/a");
            VFSManager.Move("/" + name + "/a", "/" + name + "/testFolder");
            var directory = VFSManager.getEntry("/" + name + "/testFolder") as VfsDirectory;
            var file = VFSManager.getEntry("/" + name + "/testFolder/a") as VfsFile;
            Debug.Assert(file.Parent.Address == directory.Address);
        }
    }
}
