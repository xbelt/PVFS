using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;

namespace UnitTest
{
    [TestClass]
    public class ArrayExtensionsUnitTest
    {
        [TestMethod]
        public void IndexOfFound()
        {
            var arr = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            Assert.AreEqual(arr.IndexOf("3"), 2);
        }

        [TestMethod]
        public void IndexOfNotFound()
        {
            var arr = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            Assert.AreEqual(arr.IndexOf("0"), -1);
        }
    }
}
