using System.Collections.Generic;
using System.Linq;

namespace Concordia.Spimi
{
    /// <summary>
    /// Query an index through this class.
    /// </summary>
    public class QueryEngine
    {
        private TermIndex index;
        private IndexMetadata indexMetadata;
        private BestMatchRanker bestMatchRanker;

        public QueryEngine(TermIndex index, IndexMetadata indexMetadata)
        {
            this.index = index;
            this.indexMetadata = indexMetadata;
            this.bestMatchRanker = new BestMatchRanker(index, indexMetadata);
        }

        /// <summary>
        /// Returns the documents in which a term can be found.
        /// If more than one query terms are entered, the intersection
        /// of those terms' postings lists are returned (i.e. the query string
        /// is interpreted as and "AND"-query)
        /// </summary>
        public IList<long> Query(string query)
        {
            
            PostingsDocumentIdComparer postingDocIdComparer = new PostingsDocumentIdComparer();

            string[] terms = query.Split(' ', '\t');

            HashSet<long> allHits = new HashSet<long>();

            // 1) Union the posting lists of the different terms (not AND queries anymore because we rank)
            foreach(string term in terms)
            {
                foreach (long hit in getHits(term))
                    allHits.Add(hit);
            }

            // 2) Rank the postings 
            return bestMatchRanker.Rank(terms, allHits);
        }

        private HashSet<long> getHits(string term)
        {
            HashSet<long> hits = new HashSet<long>();
            IList<Posting> foundPostings;

            if (index.TryGet(term, out foundPostings))
            {
                foreach (Posting p in foundPostings)
                {
                    hits.Add(p.DocumentId);
                }
            }

            return hits;
        }

        class PostingsDocumentIdComparer : IEqualityComparer<Posting>
        {
            public bool Equals(Posting posting1, Posting posting2)
            {
                return posting1.DocumentId.Equals(posting2.DocumentId);
            }

            public int GetHashCode(Posting posting)
            {
                return posting.DocumentId.GetHashCode();
            }
        }

        public Dictionary<long, double> Scores
        {
            get { return bestMatchRanker.Scores; }
        }
    }
}
