using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    public class TfIdfScorer : Concordia.Spimi.IScorer
    {
        private IndexMetadata indexMetadata;
        private TermIndex index;

        public TfIdfScorer(TermIndex index, IndexMetadata indexMetadata)
        {
            this.index = index;
            this.indexMetadata = indexMetadata;
        }

        public double GetQueryTermScoreContributionForDocument(long docId, string term)
        {
            IList<Posting> postings;
            if (!index.TryGet(term, out postings))
                postings = new List<Posting>();

            long N = indexMetadata.CollectionLengthInDocuments;
            int df = postings.Count;
            if (df == 0)
                return 0;
            double idf = Math.Log10(N / df);
            
            IEnumerable<Posting> docPosting = postings.Where(p => p.DocumentId == docId);
            if(docPosting.Count() == 0) 
            {
                return 0;
            }
            double tf = 1 + Math.Log10(docPosting.Single().Frequency);
            return tf * idf;
        }
    }
}
