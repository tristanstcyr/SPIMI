using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class Document
    {
        public Document(string docId, string body)
        {
            DocId = docId;
            Body = body;
        }

        public string DocId { get; private set; }
        public string Body { get; private set; }
    }
}
