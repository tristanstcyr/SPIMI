using System.Collections.Generic;
namespace Concordia.Spimi
{
    class QueryEngine
    {
        IIndex index;
        public QueryEngine(IIndex index)
        {
            this.index = index;
        }

        public IEnumerable<string> Query(string query)
        {
            foreach (string term in query.Split(' ', '\t'))
            {
                foreach (string docId in index.GetPostingList(term).Postings)
                {
                    yield return docId;
                }
            }
        }
    }
}
