using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concordia.Spimi;
using System.IO;
using System.Collections.Generic;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class SpimiBlockReaderTest
    {
        private const long
            DocA = 0,
            DocB = 1,
            DocZ = 2,
            DocD = 3;

        [TestMethod]
        public void TestRead()
        {
            SpimiBlockWriter writer = new SpimiBlockWriter();
            writer.AddPosting("aTerm", DocA);
            writer.AddPosting("aTerm", DocB);
            writer.AddPosting("bTerm", DocA);
            string filePath = writer.FlushToFile();

            SpimiBlockReader reader = new SpimiBlockReader();
            IEnumerator<PostingList> postingLists = reader.Read(filePath).GetEnumerator();
            Assert.AreEqual(true, postingLists.MoveNext());
            Assert.AreEqual("aTerm", postingLists.Current.Term);
            Assert.AreEqual(2, postingLists.Current.Postings.Count);
            Assert.AreEqual(new Posting(DocA, 1), postingLists.Current.Postings[0]);
            Assert.AreEqual(new Posting(DocB, 1), postingLists.Current.Postings[1]);
            Assert.AreEqual(true, postingLists.MoveNext());

            Assert.AreEqual("bTerm", postingLists.Current.Term);
            Assert.AreEqual(new Posting(DocA, 1), postingLists.Current.Postings[0]);
        }

        [TestMethod]
        public void TestMerge()
        {
            List<IEnumerator<PostingList>> postingLists
                = new List<IEnumerator<PostingList>>();
            
            SpimiBlockReader reader = new SpimiBlockReader();

            SpimiBlockWriter writer = new SpimiBlockWriter();
            writer.AddPosting("aTerm", DocA);
            writer.AddPosting("aTerm", DocB);
            writer.AddPosting("bTerm", DocA);
            postingLists.Add(reader.Read(writer.FlushToFile()).GetEnumerator());

            writer = new SpimiBlockWriter();
            writer.AddPosting("cTerm", DocD);
            writer.AddPosting("aTerm", DocZ);
            writer.AddPosting("bTerm", DocA);
            postingLists.Add(reader.Read(writer.FlushToFile()).GetEnumerator());

            using (IEnumerator<PostingList> merged = reader.BeginBlockMerge(postingLists).GetEnumerator())
            {
                Assert.IsTrue(merged.MoveNext());
                PostingList postingList = merged.Current;
                Assert.AreEqual("aTerm", postingList.Term);
                Assert.AreEqual(3, postingList.Postings.Count);
                Assert.AreEqual(new Posting(DocA, 1), postingList.Postings[0]);
                Assert.AreEqual(new Posting(DocB, 1), postingList.Postings[1]);
                Assert.AreEqual(new Posting(DocZ, 1), postingList.Postings[2]);

                Assert.IsTrue(merged.MoveNext());
                postingList = merged.Current;
                Assert.AreEqual("bTerm", postingList.Term);
                Assert.AreEqual(1, postingList.Postings.Count);
                Assert.AreEqual(new Posting(DocA, 2), postingList.Postings[0]);

                Assert.IsTrue(merged.MoveNext());
                postingList = merged.Current;
                Assert.AreEqual("cTerm", postingList.Term);
                Assert.AreEqual(1, postingList.Postings.Count);
                Assert.AreEqual(new Posting(DocD, 1), postingList.Postings[0]);

                Assert.IsFalse(merged.MoveNext());
            }
        }
    }
}
