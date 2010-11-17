using System.Collections.Generic;
using System.Linq;

namespace Concordia.Spimi
{
    /// <summary>
    /// Query an index through this class.
    /// </summary>
    class QueryEngine
    {
        IIndex index;
        List<Posting> queryPostings = new List<Posting>();

        public QueryEngine(IIndex index)
        {
            this.index = index;
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
            queryPostings.Clear();
            queryPostings.AddRange(index.GetPostingList(term).Postings);
            
            if (terms.Length > 1)
            {
                int i = 0;
                foreach (string andTerm in terms)
                {
                    if (i != 0)
                    {
                        IList<Posting> andTermPostings = index.GetPostingList(andTerm).Postings;
                        queryPostings = queryPostings.Intersect(andTermPostings).ToList();
                    }
                    i++;
                }
            }
            
            return queryPostings;            
        }
    }
}
