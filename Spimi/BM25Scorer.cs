using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class BM25Scorer : Concordia.Spimi.IScorer
    {
        private IndexMetadata indexMetadata;
        private TermIndex index;

        private const double k = 2.0;     // term frequency scaler
        private const double b = 0.75;   // document length scaler
        private double Lavg;

        public BM25Scorer(TermIndex index, IndexMetadata indexMetadata)
        {
            this.index = index;
            this.indexMetadata = indexMetadata;

            this.Lavg = indexMetadata.TokenCount / indexMetadata.CollectionLengthInDocuments;
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

            if (postings.Where(p => p.DocumentId == docId).Count() == 0)
            {
                return 0;
            }

            int tf = postings.Where(p => p.DocumentId == docId).Single().Frequency;
            int Ld = indexMetadata[docId].Length;
            return idf * (k + 1) * tf / (tf + k * (1 - b + b * Ld / Lavg));
        }
    }
}
