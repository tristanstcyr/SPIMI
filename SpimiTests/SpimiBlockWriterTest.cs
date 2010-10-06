using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concordia.Spimi;
using System.IO;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class SpimiBlockWriterTest
    {
        [TestMethod]
        public void TestWrite()
        {
            SpimiBlockWriter writer = new SpimiBlockWriter();
            writer.AddPosting("bTerm", "aDoc");
            writer.AddPosting("aTerm", "aDoc");
            writer.AddPosting("aTerm", "bDoc");
            string filePath = writer.FlushToFile();
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(file);
                Assert.AreEqual(2, reader.ReadInt32());
                Assert.AreEqual("aTerm", reader.ReadString());
                Assert.AreEqual(2, reader.ReadInt32());
                Assert.AreEqual("aDoc", reader.ReadString());
                Assert.AreEqual("bDoc", reader.ReadString());
                Assert.AreEqual("bTerm", reader.ReadString());
                Assert.AreEqual(1, reader.ReadInt32());
                Assert.AreEqual("aDoc", reader.ReadString());
            }
        }
    }
}
