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
    public class CollectionMetadataTests
    {

        [TestMethod]
        public void testWrite()
        {
            MemoryStream stream = new MemoryStream();
            CollectionMetadataWriter writer = 
                new CollectionMetadataWriter(stream);
            
            DocumentInfo doc1 = new DocumentInfo("http://www.google.com/index.html", "Google", 150, "#Section1", null);
            DocumentInfo doc2 = new DocumentInfo("http://www.google.com/index.html", "Google", 250, "#Section2", null);
            writer.AddDocumentInfo(0, doc1);
            writer.AddDocumentInfo(1, doc2);
            writer.WriteOut();

            BinaryReader reader = new BinaryReader(stream);
            long collectionTokenCount = reader.ReadInt64();
            Assert.AreEqual(400, collectionTokenCount);

            FileIndex<long, DocumentInfo> documentIndex = new FileIndex<long, DocumentInfo>(
                new LongEncoder(), new DocumentInfoEncoder(), stream);
            Assert.AreEqual(2, documentIndex.EntryCount);

            DocumentInfo docInfo;
            Assert.IsTrue(documentIndex.TryGet(0, out docInfo));
            Assert.AreEqual(doc1, docInfo);

            Assert.IsTrue(documentIndex.TryGet(1, out docInfo));
            Assert.AreEqual(doc2, docInfo);
        }
    }
}
