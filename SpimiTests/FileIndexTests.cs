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
    public class FileIndexTests
    {
        [TestMethod]
        public void TestWrite()
        {
            Stream stream = new MemoryStream();
            PerformWrite(stream);

            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(2, reader.ReadInt64());

            // term ptr, frequency
            reader.ReadInt64();
            Assert.AreEqual(2, reader.ReadInt32());

            // term ptr, frequency
            reader.ReadInt64();
            Assert.AreEqual(3, reader.ReadInt32());

            string[] strings = new string[] {
                "aTerm", "aDoc", "bDoc", "bTerm", "aDoc", "zDoc", "tDoc"
            };

            foreach (string str in strings)
            {
                Assert.AreEqual(str, reader.ReadString());
            }
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
            Assert.AreEqual("aDoc", list.Postings[0]);
            Assert.AreEqual("bDoc", list.Postings[1]);

            list = index.GetPostingList("bTerm");
            Assert.AreEqual("bTerm", list.Term);
            Assert.AreEqual(3, list.Postings.Count);
            Assert.AreEqual("aDoc", list.Postings[0]);
            Assert.AreEqual("zDoc", list.Postings[1]);
            Assert.AreEqual("tDoc", list.Postings[2]);
        }

        void PerformWrite(Stream stream)
        {
            FileIndexWriter writer = new FileIndexWriter();
            List<PostingList> postingLists = new List<PostingList>();

            List<string> docs = new List<string>();
            docs.Add("aDoc");
            docs.Add("bDoc");
            postingLists.Add(new PostingList("aTerm", docs));

            docs = new List<string>();
            docs.Add("aDoc");
            docs.Add("zDoc");
            docs.Add("tDoc");
            postingLists.Add(new PostingList("bTerm", docs));

            writer.Write(stream, postingLists);
        }

    }
}
