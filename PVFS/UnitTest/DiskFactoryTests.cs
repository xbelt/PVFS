using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;
using VFS.VFS.Parser;

namespace UnitTest
{
    [TestClass]
    public class DiskFactoryTests
    {
        [TestMethod]
        public void TestDiskFactoryPathWithSlash()
        {
            Directory.CreateDirectory("C:\\Test");
            var disk = DiskFactory.Create(new DiskInfo("C:\\Test\\", "a.vdi", 4096, 1024), null);
            Debug.Assert(File.Exists("C:\\Test\\a.vdi"));
            Debug.Assert(disk.DiskProperties.BlockSize == 1024);
            Debug.Assert(disk.DiskProperties.MaximumSize == 4096);
            Debug.Assert(disk.DiskProperties.Name == "a");
            Debug.Assert(disk.DiskProperties.NumberOfBlocks == 4);
            Debug.Assert(disk.DiskProperties.NumberOfUsedBlocks == 2);
            Debug.Assert(disk.DiskProperties.RootAddress == 1);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestDiskFactoryPathWithoutSlash()
        {
            Directory.CreateDirectory("C:\\Test");
            var disk = DiskFactory.Create(new DiskInfo("C:\\Test", "b.vdi", 4096, 1024), null);
            Debug.Assert(File.Exists("C:\\Test\\b.vdi"));
            Debug.Assert(disk.DiskProperties.BlockSize == 1024);
            Debug.Assert(disk.DiskProperties.MaximumSize == 4096);
            Debug.Assert(disk.DiskProperties.Name == "b");
            Debug.Assert(disk.DiskProperties.NumberOfBlocks == 4);
            Debug.Assert(disk.DiskProperties.NumberOfUsedBlocks == 2);
            Debug.Assert(disk.DiskProperties.RootAddress == 1);
            disk.Stream.Close();
        }

        [TestMethod]
        public void TestDiskFactoryLongName()
        {
            var diskName = "";
            for (int i = 0; i < 150; i++)
            {
                diskName += "a";
            }
            diskName += ".vdi";
            Directory.CreateDirectory("C:\\Test");
            var disk = DiskFactory.Create(new DiskInfo("C:\\Test", diskName, 4096, 1024), null);
            disk.Stream.Close();
            Debug.Assert(File.Exists("C:\\Test\\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.vdi"));
            Debug.Assert(disk.DiskProperties.BlockSize == 1024);
            Debug.Assert(disk.DiskProperties.MaximumSize == 4096);
            Debug.Assert(disk.DiskProperties.Name == diskName.Substring(0, 128));
            Debug.Assert(disk.DiskProperties.Name.Length == 128);
            Debug.Assert(disk.DiskProperties.NumberOfBlocks == 4);
            Debug.Assert(disk.DiskProperties.NumberOfUsedBlocks == 2);
            Debug.Assert(disk.DiskProperties.RootAddress == 1);
        }

        [TestMethod]
        public void TestDiskFactoryLoad()
        {
            string path, name;
            var createdDisk = createTestDisk(out path, out name);
            createdDisk.Stream.Close();
            var disk = DiskFactory.Load(path, null);
            Debug.Assert(createdDisk.DiskProperties.BlockSize == disk.DiskProperties.BlockSize);
            Debug.Assert(createdDisk.DiskProperties.MaximumSize == disk.DiskProperties.MaximumSize);
            Debug.Assert(createdDisk.DiskProperties.NumberOfBlocks == disk.DiskProperties.NumberOfBlocks);
            Debug.Assert(createdDisk.DiskProperties.NumberOfUsedBlocks == disk.DiskProperties.NumberOfUsedBlocks);
            Debug.Assert(createdDisk.DiskProperties.RootAddress == disk.DiskProperties.RootAddress);
            Debug.Assert(createdDisk.DiskProperties.Name == disk.DiskProperties.Name);
        }

        public static VfsDisk createTestDisk(out string path, out string name)
        {
            path = Environment.CurrentDirectory;
            name = "b";
            int i = 0;
            while (File.Exists(path + "\\" + name+".vdi"))
            {
                name = "b" + i++;
            }
            VfsDisk disk = DiskFactory.Create(new DiskInfo(path, name + ".vdi", 4096, 1024), null);
            path += "\\" + name + ".vdi";
            return disk;
        }
        public static VfsDisk createTestDisk(out string path, out string name, double size, int blocksize)
        {
            path = Environment.CurrentDirectory;
            name = "b";
            int i = 0;
            while (File.Exists(path + "\\" + name + ".vdi"))
            {
                name = "b" + i++;
            } 
            VfsDisk disk = DiskFactory.Create(new DiskInfo(path, name + ".vdi", size, blocksize), null);
            path += "\\" + name + ".vdi";
            return disk;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLoadException()
        {
            DiskFactory.Load("X::\\Test.vdi", "");
        }

        [TestMethod]
        public void TestCreateHugeDisk() {
            string path;
            string name;
            createTestDisk(out path, out name, 100000*200, 200);
        }

        [TestMethod]
        public void TestRemoveDiskSuccess()
        {
            string path;
            string name;
            var disk = createTestDisk(out path, out name, 100000 * 200, 200);
            disk.Stream.Close();
            DiskFactory.Remove(path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveDiskFailure()
        {
            string path;
            string name;
            var disk = createTestDisk(out path, out name, 100000 * 200, 200);
            disk.Stream.Close();
            DiskFactory.Remove(path + ".v");
        }
    }
}
