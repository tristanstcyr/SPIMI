using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concordia.Spimi;
using System.IO;
using System.Collections.Generic;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class SpimiBlockReaderTest
    {
        [TestMethod]
        public void TestRead()
        {
            SpimiBlockWriter writer = new SpimiBlockWriter();
            writer.AddPosting("aTerm", "aDoc");
            writer.AddPosting("aTerm", "bDoc");
            writer.AddPosting("bTerm", "aDoc");
            string filePath = writer.FlushToFile();

            SpimiBlockReader reader = new SpimiBlockReader();
            IEnumerable<PostingList> postingLists = reader.Read(filePath);
            IEnumerator<PostingList> enumerator = postingLists.GetEnumerator();
            Assert.AreEqual(true, enumerator.MoveNext());
            Assert.AreEqual("aTerm", enumerator.Current.Term);
            Assert.AreEqual("aDoc", enumerator.Current.Postings[0]);
            Assert.AreEqual("bDoc", enumerator.Current.Postings[1]);
            Assert.AreEqual(true, enumerator.MoveNext());

            Assert.AreEqual("bTerm", enumerator.Current.Term);
            Assert.AreEqual("aDoc", enumerator.Current.Postings[0]);
        }

        [TestMethod]
        public void TestMerge()
        {
            List<IEnumerator<PostingList>> postingLists
                = new List<IEnumerator<PostingList>>();
            
            SpimiBlockReader reader = new SpimiBlockReader();

            SpimiBlockWriter writer = new SpimiBlockWriter();
            writer.AddPosting("aTerm", "aDoc");
            writer.AddPosting("aTerm", "bDoc");
            writer.AddPosting("bTerm", "aDoc");
            postingLists.Add(reader.Read(writer.FlushToFile()).GetEnumerator());

            writer = new SpimiBlockWriter();
            writer.AddPosting("cTerm", "dDoc");
            writer.AddPosting("aTerm", "zDoc");
            writer.AddPosting("bTerm", "aDoc");
            postingLists.Add(reader.Read(writer.FlushToFile()).GetEnumerator());

            using (IEnumerator<PostingList> merged = reader.BeginBlockMerge(postingLists).GetEnumerator())
            {
                Assert.IsTrue(merged.MoveNext());
                PostingList postingList = merged.Current;
                Assert.AreEqual("aTerm", postingList.Term);
                Assert.AreEqual(3, postingList.Postings.Count);
                Assert.AreEqual("aDoc", postingList.Postings[0]);
                Assert.AreEqual("bDoc", postingList.Postings[1]);
                Assert.AreEqual("zDoc", postingList.Postings[2]);

                Assert.IsTrue(merged.MoveNext());
                postingList = merged.Current;
                Assert.AreEqual("bTerm", postingList.Term);
                Assert.AreEqual(1, postingList.Postings.Count);
                Assert.AreEqual("aDoc", postingList.Postings[0]);

                Assert.IsTrue(merged.MoveNext());
                postingList = merged.Current;
                Assert.AreEqual("cTerm", postingList.Term);
                Assert.AreEqual(1, postingList.Postings.Count);
                Assert.AreEqual("dDoc", postingList.Postings[0]);

                Assert.IsFalse(merged.MoveNext());
            }
        }
    }
}
