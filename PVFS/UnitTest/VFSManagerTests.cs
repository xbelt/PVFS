﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;
using VFS.VFS.Models;

namespace UnitTest
{
    [TestClass]
    public class VfsManagerTests
    {
        [TestMethod]
        public void TestLoadDisk()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
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
            accessor.InvokeStatic("LoadDisk", new [] {disk});
            var result = accessor.InvokeStatic("GetDisk", new[] {name});
            Debug.Assert(result == disk);
            disk.Stream.Close();
        }

        //----------------------Working Directory----------------------

        [TestMethod]
        public void TestChangeWorkingDirectory()
        {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/"+name+"/b/c", true);
            VfsManager.ChangeWorkingDirectory("/"+name+"/b/c");
            Assert.AreEqual(VfsManager.WorkingDirectory.AbsolutePath, "/" + name + "/b/c");
        }


        //--------------------------------------------------------------


        [TestMethod]
        public void TestGetEntryPathOnlineFile()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
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
            VfsManager.LoadDisk(disk);
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
            VfsManager.LoadDisk(disk);
            var file = VfsManager.CreateFile("/" + name + "/testFile");
            VfsManager.UnloadDisk(name);
            disk = DiskFactory.Load(path, null);
            VfsManager.LoadDisk(disk);
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
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testDir", true);
            var file = VfsManager.GetEntry("/" + name + "/testDir") as VfsDirectory;
            VfsManager.UnloadDisk(name);
            disk = DiskFactory.Load(path, null);
            VfsManager.LoadDisk(disk);
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
            VfsManager.LoadDisk(disk);
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
            accessor.InvokeStatic("LoadDisk", new[] { disk });
            accessor.InvokeStatic("UnloadDisk", new[] { name });
            var result = accessor.InvokeStatic("GetDisk", new[] { name });
            Debug.Assert(result == null);
        }

        [TestMethod]
        public void TestGetTempFilePath() {
            var accessor = new PrivateType(typeof(VfsManager));
            var result = accessor.InvokeStatic("GetTempFilePath", new object[] {});
        }

        [TestMethod]
        public void TestChangeDirectoryByIdentifier() {
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/b/c", true);
            VfsManager.ChangeWorkingDirectory("b/c");
            Assert.AreEqual(VfsManager.WorkingDirectory.AbsolutePath, "/" + name + "/b/c");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetEntryArgNullException() {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VfsManager.GetEntry(disk, null);
        }

        [TestMethod]
        public void TestGetEntryArgException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VfsManager.GetEntry("/"+name+"/");
            Debug.Assert(readFile != null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetAbsolutePathArgNullException()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            var readFile = VfsManager.GetAbsolutePath(null);
        }

        [TestMethod]
        public void TestRemoveDirectory()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VfsManager.LoadDisk(disk);
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
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            VfsManager.CreateDirectory("/" + name + "/testFolder/b", true);
            VfsManager.CreateFile("/" + name + "/testFolder/a");
            VfsManager.Remove("testFolder");
        }

        [TestMethod]
        public void TestDefrag()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateFile("/" + name + "/a");
            VfsManager.CreateFile("/" + name + "/b");
            VfsManager.CreateFile("/" + name + "/c");
            VfsManager.Remove("/" + name + "/a");
            VfsManager.Defrag();
            var lastIndex = -1;
            for (int i = 0; i < VfsManager.CurrentDisk.BitMap.Count; i++)
            {
                if (VfsManager.CurrentDisk.BitMap[i])
                {
                    Assert.AreEqual(lastIndex, i - 1);
                    lastIndex = i;
                }
            }
        }

        [TestMethod]
        public void TestGetFreeSpace()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 900, 300);
            VfsManager.LoadDisk(disk);
            VfsManager.GetFreeSpace();
            VfsManager.GetOccupiedSpace();
        }

        [TestMethod]
        public void TestExit()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 900, 300);
            VfsManager.LoadDisk(disk);
            VfsManager.Exit();
        }

        [TestMethod]
        public void TestMove()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 6000, 300);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateDirectory("/" + name + "/testFolder", true);
            VfsManager.CreateFile("/" + name + "/a");
            VfsManager.Move("/" + name + "/a", "/" + name + "/testFolder");
            var directory = VfsManager.GetEntry("/" + name + "/testFolder") as VfsDirectory;
            var file = VfsManager.GetEntry("/" + name + "/testFolder/a") as VfsFile;
            Debug.Assert(file.Parent.Address == directory.Address);
        }

        [TestMethod]
        public void TestCreateDiskOutsideDebugFolder()
        {
            //(last line of code of that function)
            var dirInfo = Directory.CreateDirectory("C:\\NewDisk");
            VfsManager.CreateDisk("C:\\NewDisk", "Disk1.vdi", 4096, 256, "pw");
            Debug.Assert(File.Exists(dirInfo.FullName + "\\" + "Disk1.vdi"));
        }

        [TestMethod]
        public void TestImportExport()
        {
            var expInfo = Directory.CreateDirectory("C:\\exportTest");
            var dirInfo1 = Directory.CreateDirectory("C:\\importTest\\a\\b");
            var dirInfo2 = Directory.CreateDirectory("C:\\importTest\\c");
            var writer1 = File.CreateText(dirInfo1.FullName + "\\f1.txt");
            writer1.Write("I'm in \\a\\b.");
            writer1.Close();
            var writer2 = File.CreateText(dirInfo2.FullName + "\\f2.txt");
            writer2.Write("I'm in \\c");
            writer2.Close();
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 16384,256);
            var diskPath = disk.Root.AbsolutePath;
            Debug.Assert(disk != null);
            VfsManager.LoadDisk(disk);
            VfsManager.Import("C:\\importTest",disk.Root.AbsolutePath);
            Debug.Assert(disk.Root.GetDirectory("importTest") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/a") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/c") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/a/b") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/a/b/f1.txt") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/c/f2.txt") != null);
            VfsManager.Export(diskPath + "/importTest", "C:\\exportTest");
            Debug.Assert(Directory.Exists("C:\\exportTest\\importTest\\a\\b"));
            Debug.Assert(Directory.Exists("C:\\exportTest\\importTest\\c"));
            Debug.Assert(File.Exists("C:\\exportTest\\importTest\\a\\b\\f1.txt"));
            Debug.Assert(File.Exists("C:\\exportTest\\importTest\\c\\f2.txt"));
            const string filePath1 = "C:\\exportTest\\importTest\\a\\b\\f1.txt";
            const string filePath2 = "C:\\exportTest\\importTest\\c\\f2.txt";
            Debug.Assert(FileContentComparer(filePath1, "C:\\importTest\\a\\b\\f1.txt"));
            Debug.Assert(FileContentComparer(filePath2, "C:\\importTest\\c\\f2.txt"));
            Directory.Delete("C:\\exportTest\\", true);
        }

        [TestMethod]
        public void TestSingleFileExport()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateFile(disk.Root.AbsolutePath + "/file.txt");
            Debug.Assert(disk.Root.GetFile("file.txt") != null);
            var directoryInfo = Directory.CreateDirectory("C:\\fileExp");
            Debug.Assert(Directory.Exists("C:\\fileExp"));
            VfsManager.Export(disk.Root.AbsolutePath + "/file.txt", "C:\\fileExp");
            Debug.Assert(File.Exists("C:\\fileExp\\file.txt"));
        }

        [TestMethod]
        public void TestSingleFileInDirectoryExport()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            var diskPath = disk.Root.AbsolutePath;
            VfsManager.CreateDirectory(diskPath + "/subdir", false);
            VfsManager.CreateFile(diskPath + "/subdir/file.txt");
            var directoryInfo = Directory.CreateDirectory("C:\\fileExp2");
            VfsManager.Export(diskPath + "/subdir", "C:\\fileExp2");
            Assert.IsTrue(File.Exists("C:\\fileExp2\\subdir\\file.txt"));
        }

        [TestMethod]

        public void TestExportWithDestToFile()
        {
            Directory.CreateDirectory("C:\\tmp\\");
            File.Create("C:\\tmp\\useless.txt");
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateFile(disk.Root.AbsolutePath + "/useless2.txt");
            VfsManager.Export(disk.Root.AbsolutePath + "/useless2.txt", "C:\\tmp\\useless.txt");
        }

        private static bool FileContentComparer(string arg1, string arg2)
        {
            byte[] file1 = File.ReadAllBytes(arg1);
            byte[] file2 = File.ReadAllBytes(arg2);
            if (file1.Length != file2.Length) return false;
            return !file1.Where((t, i) => t != file2[i]).Any();
        }

        [TestMethod]
        public void TestRename()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateFile("/" + name + "/a");
            VfsManager.Rename("/" + name + "/a", "b");
            Assert.IsNull(VfsManager.GetEntry("/" + name + "/a"));
            Assert.IsNotNull(VfsManager.GetEntry("/" + name + "/b"));
            Assert.IsTrue(VfsManager.GetEntry("/" + name + "/b").Name == "b");
        }

        [TestMethod]
        public void TestCopy()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            VfsManager.LoadDisk(disk);
            VfsManager.CreateFile("/" + name + "/a");
            VfsManager.Console = new MockConsole {queryReturn = 0};
            VfsManager.Copy("/" + name + "/a", "/" + name + "/b");
            Assert.IsNotNull(VfsManager.GetEntry("/" + name + "/a"));
            Assert.IsNotNull(VfsManager.GetEntry("/" + name + "/b/a"));
        }

        [TestMethod]
        public void TestHelp()
        {
            VfsManager.Help();
        }

        [TestMethod]
        public void TestEncryptionDecryption()
        {
            Directory.CreateDirectory("C:\\exportTest");
            var dirInfo1 = Directory.CreateDirectory("C:\\importTest\\a\\b");
            var dirInfo2 = Directory.CreateDirectory("C:\\importTest\\c");
            var writer1 = File.CreateText(dirInfo1.FullName + "\\f1.txt");
            writer1.Write("I'm in \\a\\b.");
            writer1.Close();
            var writer2 = File.CreateText(dirInfo2.FullName + "\\f2.txt");
            writer2.Write("I'm in \\c");
            writer2.Close();
            string path, name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 16384, 256, "abc");
            var diskPath = disk.Root.AbsolutePath;
            Debug.Assert(disk != null);
            VfsManager.LoadDisk(disk);
            VfsManager.Import("C:\\importTest", disk.Root.AbsolutePath);
            Debug.Assert(disk.Root.GetDirectory("importTest") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/a") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/c") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/a/b") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/a/b/f1.txt") != null);
            Debug.Assert(VfsManager.GetEntry(diskPath + "/importTest/c/f2.txt") != null);
            VfsManager.Export(diskPath + "/importTest", "C:\\exportTest");
            Debug.Assert(Directory.Exists("C:\\exportTest\\importTest\\a\\b"));
            Debug.Assert(Directory.Exists("C:\\exportTest\\importTest\\c"));
            Debug.Assert(File.Exists("C:\\exportTest\\importTest\\a\\b\\f1.txt"));
            Debug.Assert(File.Exists("C:\\exportTest\\importTest\\c\\f2.txt"));
            const string filePath1 = "C:\\exportTest\\importTest\\a\\b\\f1.txt";
            const string filePath2 = "C:\\exportTest\\importTest\\c\\f2.txt";
            Debug.Assert(FileContentComparer(filePath1, "C:\\importTest\\a\\b\\f1.txt"));
            Debug.Assert(FileContentComparer(filePath2, "C:\\importTest\\c\\f2.txt"));
            Directory.Delete("C:\\exportTest\\", true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestListEntriesException()
        {
            var path = Environment.CurrentDirectory;
            var name = "b";
            int i = 0;
            while (File.Exists(path + "\\" + name + ".vdi"))
            {
                name = "b" + i++;
            }
            VfsManager.ExecuteCommand("cdisk -n " + name + " -s 1mb");
            VfsManager.CreateDirectory("a/b/c", true);
            VfsManager.CreateFile("a.txt");
            VfsManager.ListEntries("b", true, true);
            VfsManager.ListEntries("a.txt", true, true);
            VfsManager.ListEntries("", true, true);
            VfsManager.ListEntries(null, true, true);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void TestMoveException1()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            VfsManager.LoadDisk(disk);
            VfsManager.Move(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestMoveException2()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            VfsManager.LoadDisk(disk);
            VfsManager.Move("", null);
        }

        [TestMethod]
        public void TestUnopenedDiskExceptions()
        {
            VfsManager.Console = new MockConsole();
            VfsManager.Move("", "");
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.NavigateUp();
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.ListEntries("", true, true);
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.CreateDirectory("", true);
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.CreateFile("");
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.Rename("", "");
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.Copy("", "");
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.Remove("");
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.Import("", "");
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.Export("", "");
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.GetFreeSpace();
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.GetOccupiedSpace();
            Assert.AreEqual(((MockConsole) VfsManager.Console).lastCommand, MockConsole.Command.Error);
        }

        [TestMethod]
        public void TestMoveErrors()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            VfsManager.LoadDisk(disk);
            VfsManager.Console = new MockConsole();
            VfsManager.CreateFile("a.txt");
            VfsManager.CreateDirectory("a", true);
            VfsManager.Move("b.txt", "");
            Assert.AreEqual(((MockConsole)VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.Move("a.txt", "a.txt");
            Assert.AreEqual(((MockConsole)VfsManager.Console).lastCommand, MockConsole.Command.Error);
            VfsManager.Move("/" + name, "a");
            Assert.AreEqual(((MockConsole)VfsManager.Console).lastCommand, MockConsole.Command.Error);
        }

        [TestMethod]
        public void TestCreateDirectoryErrors()
        {
            string path;
            string name;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 1000, 200);
            VfsManager.LoadDisk(disk);
            VfsManager.Console = new MockConsole();
            VfsManager.CreateDirectory("a", true);
            VfsManager.CreateDirectory("a", true);
            Assert.AreEqual(((MockConsole)VfsManager.Console).lastCommand, MockConsole.Command.Message);
            VfsManager.CreateFile("b");
            VfsManager.CreateDirectory("b", true);
            Assert.AreEqual(((MockConsole)VfsManager.Console).lastCommand, MockConsole.Command.Error);
        }


        public class MockConsole : VfsConsole
        {
            public enum Command
            {
                Error,
                Message,
                Query,
                Readline
            };

            public Command lastCommand;
            public int queryReturn = 0;
            public string readLineReturn = "";

            public override void ErrorMessage(string message)
            {
                lastCommand = Command.Error;
            }

            public override void Message(string message)
            {
                lastCommand=Command.Message;
            }

            public override int Query(string message, params string[] options)
            {
                lastCommand = Command.Query;
                return queryReturn;
            }

            public override string Readline(string message)
            {
                lastCommand = Command.Readline;
                return readLineReturn;
            }
        }
    }
}
