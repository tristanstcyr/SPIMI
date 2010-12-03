using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Concordia.Spimi;

namespace Concordia.SpimiTests
{
    [TestClass]
    public class HtmlParserTests
    {
        string htmlDoc = @"
            <html>
                <head>
                    <title>Lazy dog</title>
                </head>
                <body>
                    <h2>jumps </h2>
                    <h3>over</h3>
                    <p>the crazy <p>frog</p></p>
                </body>
            </html>";

        [TestMethod]
        public void TestParse()
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(htmlDoc);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Seek(0, SeekOrigin.Begin);
            HtmlParser parser = new HtmlParser();
            IEnumerable<Document> docs = parser.ExtractDocuments(stream);
            Document doc = docs.First();
            Assert.AreEqual("Lazy dog jumps over the crazy frog", doc.Body);
            Assert.AreEqual("Lazy dog", doc.Title);
        }
    }
}
