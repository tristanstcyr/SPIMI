using System.Collections.Generic;
using System.Linq;

namespace Concordia.Spimi
{
    class MemoryIndex : IIndex
    {
        Dictionary<string, HashSet<string>> index = new Dictionary<string, HashSet<string>>();

        public void AddTerm(string term, string docId)
        {
            HashSet<string> postingList;
            if (!index.TryGetValue(term, out postingList))
            {
                postingList = new HashSet<string>();
                index.Add(term, postingList);
            }
            postingList.Add(docId);
        }

        public PostingList GetPostingList(string term)
        {
            HashSet<string> postingList;
            if (index.TryGetValue(term, out postingList))
            {
                return new PostingList(term, postingList.ToList());
            }
            else
            {
                return new PostingList(term, new List<string>());
            }
        }
    }
}
