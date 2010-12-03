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
    public class QueryEngineTests
    {
        TermIndex index;
        IndexMetadata metadata;

        IList<Posting> postingsWithFoo;

        IList<Posting> postingsWithBar;

        [TestInitialize]
        public void before()
        {
            MemoryStream termIndexStream = new MemoryStream();
            using (FileIndexWriter<string, IList<Posting>> indexWriter = new FileIndexWriter<string, IList<Posting>>(
                new StringEncoder(), new PostingListEncoder(), termIndexStream))
            {
                this.postingsWithBar = new List<Posting>();
                postingsWithBar.Add(new Posting(0, 1));
                postingsWithBar.Add(new Posting(1, 2));
                indexWriter.Add("bar", postingsWithBar);

                this.postingsWithFoo = new List<Posting>();
                postingsWithFoo.Add(new Posting(0, 4));
                indexWriter.Add("foo", postingsWithFoo);

                indexWriter.WriteOut();
            }
            this.index = new TermIndex(termIndexStream);

            MemoryStream metadataStream = new MemoryStream();
            using (CollectionMetadataWriter metadataWriter = new CollectionMetadataWriter(metadataStream))
            {
                metadataWriter.AddDocumentInfo(0, new DocumentInfo("http://www.example.com/index.html", "Example", 100, ""));
                metadataWriter.AddDocumentInfo(1, new DocumentInfo("http://www.example.com/menu.html", "Example", 300, ""));
                metadataWriter.WriteOut();
            }

            this.metadata = new IndexMetadata(metadataStream);
        }

        [TestMethod]
        public void testQuery()
        {
            QueryEngine engine = new QueryEngine(index, metadata);
            IList<long> foundPostings = engine.Query("foo bar");
            IList<Posting> expectedPostings = postingsWithFoo.Union(postingsWithBar).ToList();
            foreach (Posting posting in expectedPostings)
            {
                Assert.IsTrue(foundPostings.Contains(posting.DocumentId));
            }
        }
    }
}
