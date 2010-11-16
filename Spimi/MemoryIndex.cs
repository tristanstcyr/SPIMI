using System.Collections.Generic;
using System.Linq;

namespace Concordia.Spimi
{
    class MemoryIndex : IIndex
    {
        // Term -> <DocId ->  (DocId, count)>
        Dictionary<string, Dictionary<string, Posting>> index = new Dictionary<string, Dictionary<string, Posting>>();

        public int Postings { get; private set; }

        public IList<string> Vocabulary
        {
            get
            {
                return index.Keys.ToList();
            }
        }

        public MemoryIndex()
        {
            this.Postings = 0;
        }

        public void AddTerm(string term, string docId)
        {
            // Get the posting list
            Dictionary<string, Posting> postingList;
            // If we encounter a term for the first time
            if (!index.TryGetValue(term, out postingList))
            {
                postingList = new Dictionary<string, Posting>();
                index.Add(term, postingList);
            }

            // Get the posting
            Posting posting;
            // If we encounter a docId for the first time
            if (!postingList.TryGetValue(docId, out posting))
            {
                posting = new Posting(docId, 0);
                postingList.Add(docId, posting);
            }

            posting.Frequency += 1;

            this.Postings++;
        }

        public PostingList GetPostingList(string term)
        {
            Dictionary<string, Posting> postingList;
            if (index.TryGetValue(term, out postingList))
            {
                return new PostingList(term, postingList.Values.ToList());
            }
            else
            {
                // Term was not found
                return new PostingList(term, new List<Posting>());
            }
        }
    }
}
