using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concordia.Spimi;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class FileIndexTests
    {
        [TestMethod]
        public void TestWrite()
        {
            Stream stream = new MemoryStream();
            PerformWrite(stream);

            BinaryReader reader = new BinaryReader(stream);
            
            // Term count
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(2, reader.ReadInt64());

            // (8) Term ptr, (4) Doc Frequency
            reader.ReadInt64();
            Assert.AreEqual(2, reader.ReadInt32());

            // (8) Term ptr, (4) Doc Frequency
            reader.ReadInt64();
            Assert.AreEqual(3, reader.ReadInt32());

            BinaryFormatter formatter = new BinaryFormatter();
            Assert.AreEqual("aTerm", reader.ReadString());
            Assert.AreEqual(new Posting("aDoc", 1), BinaryReaderTestHelper.readPosting(reader));
            Assert.AreEqual(new Posting("bDoc", 1), BinaryReaderTestHelper.readPosting(reader));
            Assert.AreEqual("bTerm", reader.ReadString());
            Assert.AreEqual(new Posting("aDoc", 1), BinaryReaderTestHelper.readPosting(reader));
            Assert.AreEqual(new Posting("zDoc", 1), BinaryReaderTestHelper.readPosting(reader));
            Assert.AreEqual(new Posting("tDoc", 1), BinaryReaderTestHelper.readPosting(reader));
        }

        [TestMethod]
        public void TestRead()
        {
            Stream stream = new MemoryStream();
            PerformWrite(stream);
            FileIndex index = FileIndex.Open(stream);
            
            Assert.AreEqual(2, index.TermCount);
            PostingList list = index.GetPostingList("aTerm");
            Assert.AreEqual("aTerm", list.Term);
            Assert.AreEqual(2, list.Postings.Count);
            Assert.AreEqual(new Posting("aDoc", 1), list.Postings[0]);
            Assert.AreEqual(new Posting("bDoc", 1), list.Postings[1]);

            list = index.GetPostingList("bTerm");
            Assert.AreEqual("bTerm", list.Term);
            Assert.AreEqual(3, list.Postings.Count);
            Assert.AreEqual(new Posting("aDoc", 1), list.Postings[0]);
            Assert.AreEqual(new Posting("zDoc", 1), list.Postings[1]);
            Assert.AreEqual(new Posting("tDoc", 1), list.Postings[2]);
        }

        void PerformWrite(Stream stream)
        {
            FileIndexWriter writer = new FileIndexWriter();
            List<PostingList> postingLists = new List<PostingList>();

            List<Posting> postings = new List<Posting>();
            postings.Add(new Posting("aDoc", 1));
            postings.Add(new Posting("bDoc", 1));
            postingLists.Add(new PostingList("aTerm", postings));

            postings = new List<Posting>();
            postings.Add(new Posting("aDoc", 1));
            postings.Add(new Posting("zDoc", 1));
            postings.Add(new Posting("tDoc", 1));
            postingLists.Add(new PostingList("bTerm", postings));

            writer.Write(stream, postingLists);
        }

    }
}
