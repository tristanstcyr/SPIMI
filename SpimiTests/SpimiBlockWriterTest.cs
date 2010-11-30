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
    public class SpimiBlockWriterTest
    {

        private const long
            DocA = 0,
            DocB = 1;

        [TestMethod]
        public void TestWrite()
        {
            PostingListEncoder decoder = new PostingListEncoder();
            SpimiBlockWriter writer = new SpimiBlockWriter();
            writer.AddPosting("bTerm", DocA);
            writer.AddPosting("aTerm", DocA);
            writer.AddPosting("aTerm", DocB);
            string filePath = writer.FlushToFile();
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(file);
                Assert.AreEqual(2, reader.ReadInt32());
                Assert.AreEqual("aTerm", reader.ReadString());
                IList<Posting> postings = new List<Posting>();
                postings.Add(new Posting(DocA, 1));
                postings.Add(new Posting(DocB, 1));
                IList<Posting> readPostings = decoder.read(reader);
                for (int i = 0; i < postings.Count; i++ )
                {
                    readPostings[i].Equals(postings[i]);
                }
                Assert.AreEqual("bTerm", reader.ReadString());
                readPostings = decoder.read(reader);
                Assert.AreEqual(new Posting(DocA, 1), readPostings[0]);
            }
        }
    }
}
