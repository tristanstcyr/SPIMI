using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concordia.Spimi;
using System.IO;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class PostingListEncoderTests
    {

        private const long
            DocA = 133,
            DocB = 150;

        [TestMethod]
        public void TestReadWrite()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            BinaryReader reader = new BinaryReader(stream);
            PostingListEncoder encoder = new PostingListEncoder();
            List<Posting> postings = new List<Posting>();
            postings.Add(new Posting(DocA, 23));
            postings.Add(new Posting(DocB, 23));

            encoder.write(writer, postings);

            stream.Seek(0, SeekOrigin.Begin);
            IList<Posting> readPostings = encoder.read(reader);

            Assert.AreEqual(postings.Count, readPostings.Count);
            for (int postingIndex = 0; postingIndex < postings.Count; postingIndex++)
            {
                Assert.AreEqual(postings[postingIndex].Frequency, readPostings[postingIndex].Frequency);
                Assert.AreEqual(postings[postingIndex].DocumentId, readPostings[postingIndex].DocumentId);
            }
        }
    }
}
