using System.Collections.Generic;
using System.Linq;

namespace Concordia.Spimi
{
    /// <summary>
    /// Query an index through this class.
    /// </summary>
    class QueryEngine
    {
        private IIndex index;
        private List<Posting> hits = new List<Posting>();
        private IndexMetadata indexMetadata;
        private BestMatchRanker bestMatchRanker;

        public QueryEngine(IIndex index, IndexMetadata indexMetadata)
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
        public IList<Posting> Query(string query)
        {
            string[] terms = query.Split(' ', '\t');

            string term = terms[0];
            hits.Clear();
            hits.AddRange(index.GetPostingList(term).Postings.ToList());

            
            if (terms.Length > 1)
            {
                // 1) Union the posting lists of the different terms (not AND queries anymore because we rank)
                int i = 0;
                foreach (string andTerm in terms)
                {
                    if (i != 0)
                    {
                        IList<Posting> andTermPostings = index.GetPostingList(andTerm).Postings.ToList();
                        hits = hits.Union(andTermPostings).ToList();  // relies on the fact that GetHashCode is overriden on Posting and returns the docId
                    }
                    i++;
                }
            }

            // 2) Rank the postings 
            return bestMatchRanker.Rank(terms, hits);
        }

        public Dictionary<string, double> Scores
        {
            get { return bestMatchRanker.Scores; }
        }
    }
}
