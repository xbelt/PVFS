using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;

namespace UnitTest
{
    [TestClass]
    public class DiskFactoryTests
    {
        [TestMethod]
        public void TestDiskFactoryPathWithSlash()
        {
            Directory.CreateDirectory("C:\\Test");
            var disk = DiskFactory.Create(new DiskInfo("C:\\Test\\", "a.vdi", 4096, 1024), "");
            Debug.Assert(File.Exists("C:\\Test\\a.vdi"));
            Debug.Assert(disk.DiskProperties.BlockSize == 1024);
            Debug.Assert(disk.DiskProperties.MaximumSize == 4096);
            Debug.Assert(disk.DiskProperties.Name == "a");
            Debug.Assert(disk.DiskProperties.NumberOfBlocks == 4);
            Debug.Assert(disk.DiskProperties.NumberOfUsedBlocks == 2);
            Debug.Assert(disk.DiskProperties.RootAddress == 1);
        }

        [TestMethod]
        public void TestDiskFactoryPathWithoutSlash()
        {
            Directory.CreateDirectory("C:\\Test");
            var disk = DiskFactory.Create(new DiskInfo("C:\\Test", "b.vdi", 4096, 1024), "");
            Debug.Assert(File.Exists("C:\\Test\\b.vdi"));
            Debug.Assert(disk.DiskProperties.BlockSize == 1024);
            Debug.Assert(disk.DiskProperties.MaximumSize == 4096);
            Debug.Assert(disk.DiskProperties.Name == "b");
            Debug.Assert(disk.DiskProperties.NumberOfBlocks == 4);
            Debug.Assert(disk.DiskProperties.NumberOfUsedBlocks == 2);
            Debug.Assert(disk.DiskProperties.RootAddress == 1);
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
            var disk = DiskFactory.Create(new DiskInfo("C:\\Test", diskName, 4096, 1024), "");
            Debug.Assert(File.Exists("C:\\Test\\" + diskName.Substring(0, 128) + ".vdi"));
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
            string path;
            var createdDisk = createTestDisk(out path);
            createdDisk.Stream.Close();
            var disk = DiskFactory.Load(path, "");
            Debug.Assert(createdDisk.DiskProperties.BitMapOffset == disk.DiskProperties.BitMapOffset);
            Debug.Assert(createdDisk.DiskProperties.BlockSize == disk.DiskProperties.BlockSize);
            Debug.Assert(createdDisk.DiskProperties.MaximumSize == disk.DiskProperties.MaximumSize);
            Debug.Assert(createdDisk.DiskProperties.NumberOfBlocks == disk.DiskProperties.NumberOfBlocks);
            Debug.Assert(createdDisk.DiskProperties.NumberOfUsedBlocks == disk.DiskProperties.NumberOfUsedBlocks);
            Debug.Assert(createdDisk.DiskProperties.RootAddress == disk.DiskProperties.RootAddress);
            Debug.Assert(createdDisk.DiskProperties.Name == disk.DiskProperties.Name);
        }

        public static VfsDisk createTestDisk(out string path)
        {
            Directory.CreateDirectory("C:\\Test");
            path = "C:\\Test\\b.vdi";
            return DiskFactory.Create(new DiskInfo("C:\\Test", "b.vdi", 4096, 1024), "");
        }
    }
}
