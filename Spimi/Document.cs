using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class Document
    {
        public Document(string docId, string title, string body)
        {
            this.SpecialIdentifier = docId;
            this.Body = body;
            this.Title = title;
        }

        public string SpecialIdentifier { get; private set; }
        public string Body { get; private set; }
        public string Title { get; private set; }
    }
}
