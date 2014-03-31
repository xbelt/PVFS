using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;

namespace UnitTest
{
    [TestClass]
    public class VFSManagerTests
    {
        [TestMethod]
        public void TestAddAndOpenDisk()
        {
            string path;
            var disk = DiskFactoryTests.createTestDisk(out path);
            VFSManager.AddAndOpenDisk(disk);
            Debug.Assert(VFSManager.CurrentDisk == disk);
            Debug.Assert(VFSManager.workingDirectory == disk.root);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestGetDisk()
        {
            string path;
            var disk = DiskFactoryTests.createTestDisk(out path);

            var accessor = new PrivateType(typeof (VFSManager));
            accessor.InvokeStatic("AddAndOpenDisk", new [] {disk});
            var result = accessor.InvokeStatic("getDisk", new[] {"b"});
            Debug.Assert(result == disk);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestGetEntryPathOnlineFile()
        {
            
        }

        [TestMethod]
        public void TestGetEntryPathOnlineFolder()
        {
            
        }

        [TestMethod]
        public void TestGetEntryPathOfflineFile()
        {

        }

        [TestMethod]
        public void TestGetEntryPathOfflineFolder()
        {

        }

        [TestMethod]
        public void TestGetAbsolutePath()
        {
            string path;
            var disk = DiskFactoryTests.createTestDisk(out path);
            VFSManager.AddAndOpenDisk(disk);
            Debug.Assert(VFSManager.getAbsolutePath("") == "/b");
            Debug.Assert(VFSManager.getAbsolutePath(".") == "/b");
            Debug.Assert(VFSManager.getAbsolutePath("..") == "/b");
            Debug.Assert(VFSManager.getAbsolutePath("/b") == "/b");
            VFSManager.CreateDirectory("/b/a", true);
            Debug.Assert(VFSManager.getAbsolutePath("a") == "/b/a");
            disk.Stream.Close();
        }
    }
}
