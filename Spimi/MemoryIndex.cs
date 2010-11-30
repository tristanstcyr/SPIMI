using System.Collections.Generic;
//using System.Linq;

namespace Concordia.Spimi
{
    class MemoryIndex : IIndex<string, IList<Posting>>
    {
        // Term -> <DocId ->  (DocId, count)>
        SortedList<string, SortedList<long, Posting>> index = new SortedList<string, SortedList<long, Posting>>();

        public int Postings { get; private set; }

        public SortedList<string, SortedList<long, Posting>> Entries
        {
            get
            {
                return index;
            }
        }

        public MemoryIndex()
        {
            this.Postings = 0;
        }

        public void AddTerm(string term, long docId)
        {
            // Get the posting list
            SortedList<long, Posting> postingList;
            // If we encounter a term for the first time
            if (!index.TryGetValue(term, out postingList))
            {
                postingList = new SortedList<long, Posting>();
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

        public bool TryGet(string key, out IList<Posting> value)
        {
            value = null;
            SortedList<long, Posting> postings = null;
            if (!index.TryGetValue(key, out postings))
            {
                return false;
            }
            return true;
        }
    }
}
