using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;

namespace Concordia.Spimi.Net.Tests
{
    [TestClass]
    public class TokenizerTest
    {
        [TestMethod]
        public void BasicLexer_ShouldBeSane()
        {
            //BasicLexer tokenizer = new BasicLexer();
            //Stream stream = new MemoryStream(ASCIIEncoding.ASCII.GetBytes("hello    \n\tworld"));
            //string[] result = tokenizer.tokenize(stream).ToArray();
            //Assert.AreEqual("hello", result[0]);
            //Assert.AreEqual("world", result[1]);
        }

        [TestMethod]
        public void BasicLexer_ShouldHandleLargeFilesFromReutersPrettyFast()
        {
            //BasicLexer tokenizer = new BasicLexer();
            //Stream stream = new FileStream("../../../SpimiTests/TestData/reuterstest.xml", FileMode.Open);
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //string[] result = tokenizer.tokenize(stream).ToArray();
            //Assert.IsTrue(result.Count() > 100000);                     // more than 100 000 tokens
            //Assert.IsTrue(stopwatch.Elapsed.Milliseconds < 500);        // in under 500 ms
        }


    }
}
