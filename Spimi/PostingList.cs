using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class PostingList
    {
        public string Term { get; private set; }

        public IList<Posting> Postings { get; private set; }

        public PostingList(string term, IList<Posting> postings)
        {
            this.Term = term;
            this.Postings = postings;
        }
    }
}
