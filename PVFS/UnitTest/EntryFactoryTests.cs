using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;

namespace UnitTest
{
    [TestClass]
    public class EntryFactoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCreateFileDiskArgNullException()
        {
            EntryFactory.createFile(null, "", 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCreateFileParentArgNullException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            EntryFactory.createFile(disk, "", 0, null);
            disk.Stream.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateFileNameLengthArgumentException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            EntryFactory.createFile(disk, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 0, null);
            disk.Stream.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateFileNegativeLength()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            EntryFactory.createFile(disk, "aa", -1, null);
            disk.Stream.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateFileFullDisk()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 2048, 1024);
            EntryFactory.createFile(disk, "a", 500, disk.Root);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestCreateFileMultipleBlockAllocation() {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            EntryFactory.createFile(disk, "a", 400, disk.Root);
            disk.Stream.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCreateDirectoryDiskArgNullException()
        {
            EntryFactory.createDirectory(null, "", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCreateDirectoryParentArgNullException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            EntryFactory.createDirectory(disk, "", null);
            disk.Stream.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateDirectoryNameLengthArgumentException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            EntryFactory.createDirectory(disk, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", null);
            disk.Stream.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateDirectoryFullDisk()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 2048, 1024);
            EntryFactory.createDirectory(disk, "a", new VfsDirectory(disk, 1, "b", null, 1, 1, 0));
            disk.Stream.Close();
        }
    }
}
