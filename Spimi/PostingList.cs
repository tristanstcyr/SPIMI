using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class PostingList
    {
        public string Term { get; private set; }

        public List<string> Postings { get; private set; }

        public PostingList(string term, List<string> postings)
        {
            this.Term = term;
            this.Postings = postings;
        }
    }
}
