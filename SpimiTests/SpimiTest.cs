using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concordia.Spimi;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class SpimiTest
    {
        const string TestData1 = @"
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce venenatis justo vitae arcu 
            convallis sit amet vestibulum eros hendrerit. Etiam pellentesque arcu id dui cursus eu pharetra elit pharetra. Sed metus";

        const string TestData2 = @"
            Pellentesque augue leo, ornare nec sodales vitae, vestibulum et nisi. Donec ac est odio, non rutrum odio. Curabitur nec blandit augue. 
            Quisque a nibh vel augue placerat mattis nec sit amet nisi. Praesent eget lectus mauris, non sollicitudin elit. Ut suscipit nisl vel mauris 
            interdum vitae faucibus quam dignissim. Vivamus ut leo ac lacus aliquam feugiat sed ac ipsum. Aliquam in sollicitudin purus. Phasellus mauris nibh, 
            blandit vitae interdum eget, eleifend quis nisi. Nunc nec dui mauris, quis gravida diam. Nulla tempor arcu ut neque euismod quis accumsan ante dictum. Suspendisse lacus risus, 
            iaculis id fringilla non, imperdiet nec nisi. Nulla at metus ac eros pulvinar posuere quis nec nibh. Pellentesque malesuada tincidunt diam, sed elementum lectus fermentum convallis. 
            Mauris tristique magna eu erat semper volutpat. Nullam interdum pr";

        [TestMethod]
        [DeploymentItem(@"LoremIpsum.txt")]
        public void TestLargeIndex()
        {
            SpimiIndexer spimi = new SpimiIndexer(new BasicLexer(), new ReutersParser());

            int sitTestData1Ocurrences = 
                new Regex("sit", RegexOptions.IgnoreCase).Matches(TestData1).Count;
            int sitTestData2Ocurrences = 
                new Regex("sit", RegexOptions.IgnoreCase).Matches(TestData2).Count;


            MemoryStream indexStream = new MemoryStream();
            spimi.Index("TestData1", GetStream(TestData1));
            spimi.Index("TestData2", GetStream(TestData2));
            spimi.MergeIndexBlocks(indexStream);
            FileIndex index = FileIndex.Open(indexStream);
            PostingList list = index.GetPostingList("sit");
            Assert.AreEqual("sit", list.Term);
            Assert.AreEqual(new Posting("TestData1", sitTestData1Ocurrences), list.Postings[0]);
            Assert.AreEqual(new Posting("TestData2", sitTestData2Ocurrences), list.Postings[1]);
        }

        Stream GetStream(string str)
        {
            return new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(str));
        }
    }
}
