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

        private const long
            DocA = 0,
            DocB = 1,
            DocZ = 2,
            DocT = 3;

        [TestMethod]
        public void TestWrite()
        {
            MemoryStream stream = new MemoryStream();
            
            // FileIndex should support a stream starting at any point
            stream.Seek(10, SeekOrigin.Begin);

            PostingListEncoder decoder = new PostingListEncoder();
            PerformWrite(stream);

            BinaryReader reader = new BinaryReader(stream);

            // Term count
            Assert.AreEqual(2, reader.ReadInt64());

            long ptr1 = reader.ReadInt64();
            long ptr2 = reader.ReadInt64();

            Assert.AreEqual(stream.Position, ptr1);
            Assert.AreEqual("aTerm", reader.ReadString());
            IList<Posting> postings = decoder.read(reader);
            Assert.AreEqual(2, postings.Count);
            Assert.AreEqual(new Posting(DocA, 1), postings[0]);
            Assert.AreEqual(new Posting(DocB, 1), postings[1]);
            
            Assert.AreEqual(stream.Position, ptr2);
            Assert.AreEqual("bTerm", reader.ReadString());
            postings = decoder.read(reader);
            Assert.AreEqual(3, postings.Count);
            Assert.AreEqual(new Posting(DocA, 1), postings[0]);
            Assert.AreEqual(new Posting(DocZ, 1), postings[1]);
            Assert.AreEqual(new Posting(DocT, 1), postings[2]);
        }

        [TestMethod]
        public void TestRead()
        {
            Stream stream = new MemoryStream();
            // FileIndex should support a stream starting at any point
            stream.Seek(100, SeekOrigin.Begin);

            PerformWrite(stream);
            TermIndex index =
                new TermIndex(stream);

            Assert.AreEqual(100, stream.Position);

            Assert.AreEqual(2, index.EntryCount);
            IList<Posting> postings;
            index.TryGet("aTerm", out postings);
            Assert.IsNotNull(postings);
            Assert.AreEqual(2, postings.Count);
            Assert.AreEqual(new Posting(DocA, 1), postings[0]);
            Assert.AreEqual(new Posting(DocB, 1), postings[1]);

            index.TryGet("bTerm", out postings);
            Assert.IsNotNull(postings);
            Assert.AreEqual(3, postings.Count);
            Assert.AreEqual(new Posting(DocA, 1), postings[0]);
            Assert.AreEqual(new Posting(DocZ, 1), postings[1]);
            Assert.AreEqual(new Posting(DocT, 1), postings[2]);
        }

        void PerformWrite(Stream stream)
        {
            long previousPosition = stream.Position;
            FileIndexWriter<string, IList<Posting>> writer = new FileIndexWriter<string, IList<Posting>>(
                new StringEncoder(),
                new PostingListEncoder(),
                stream);

            List<Posting> postings = new List<Posting>();
            postings.Add(new Posting(DocA, 1));
            postings.Add(new Posting(DocB, 1));
            writer.Add("aTerm", postings);

            postings = new List<Posting>();
            postings.Add(new Posting(DocA, 1));
            postings.Add(new Posting(DocZ, 1));
            postings.Add(new Posting(DocT, 1));
            writer.Add("bTerm", postings);

            string file = Path.GetTempFileName();
            writer.WriteOut();
            Assert.AreEqual(previousPosition, stream.Position);
        }

    }
}
