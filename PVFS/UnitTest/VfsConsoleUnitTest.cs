using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;

namespace UnitTest
{
    [TestClass]
    public class VfsConsoleUnitTest
    {
        [TestMethod]
        public void TestMessageColor()
        {
            var console = new VfsConsole();
            console.Message("Test", ConsoleColor.DarkRed);
        }

        [TestMethod]
        public void TestQuery()
        {
            var console = new VfsConsole();
            Console.SetIn(new StringReader("1"));
            Assert.AreEqual(console.Query("Please choose a number between 0 and 4", new[] {"0", "1", "2", "3", "4"}), 1);
        }

        [TestMethod]
        public void TestReadline()
        {
            var console = new VfsConsole();
            Console.SetIn(new StringReader("4"));
            Assert.AreEqual("4", console.Readline("Test"));
        }
    }
}
