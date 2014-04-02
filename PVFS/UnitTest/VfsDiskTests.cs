using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class VfsDiskTests
    {
        [TestMethod]
        public void TestAllocateOnFullDisk() {
            string name;
            string path;
            var disk = DiskFactoryTests.createTestDisk(out path, out name, 2048, 1024);
            int address;
            Debug.Assert(!disk.Allocate(out address));
            Debug.Assert(address == 0);
        }
    }
}
