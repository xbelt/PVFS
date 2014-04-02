using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFS.VFS;

namespace UnitTest
{
    [TestClass]
    public class VfsFileTest
    {
        [TestMethod]
        public void TestgetType()
        {   //You have to have a local disk with name b and a file fs.txt in root directory
            const string path = "C:\\Test\\b.vdi";
            var disk = DiskFactory.Load(path, "a");
            Assert.AreNotSame(null, disk);
            Assert.AreNotSame(null, disk.root);
            Assert.AreNotSame(null, disk.root.GetFile("fs.txt"));
            Assert.AreEqual("txt", disk.root.GetFile("fs.txt").Type);
        }
    }
}
