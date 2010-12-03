using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concordia.Spimi;
using System.IO;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class DocumentInfoEncoderTests
    {
        [TestMethod]
        public void testEncodeDecode()
        {
            MemoryStream stream = new MemoryStream();
            DocumentInfoEncoder encoder = new DocumentInfoEncoder();

            DocumentInfo writtenDoc = new DocumentInfo("http://www.google.com/index.html", "Google", 150, "#Section1");
            encoder.write(new BinaryWriter(stream), writtenDoc);

            stream.Seek(0, SeekOrigin.Begin);

            DocumentInfo readDoc = encoder.read(new BinaryReader(stream));
            Assert.AreEqual(writtenDoc, readDoc);
        }
    }
}
