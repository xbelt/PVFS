using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Debug.Assert(VFSManager.workingDirectory == disk.root);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestGetDisk()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);

            var accessor = new PrivateType(typeof (VFSManager));
            accessor.InvokeStatic("AddAndOpenDisk", new [] {disk});
            var result = accessor.InvokeStatic("getDisk", new[] {"b"});
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
            Assert.AreEqual(VFSManager.workingDirectory.GetAbsolutePath(), "/b/b/c");
        }


        //--------------------------------------------------------------


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
    }
}
