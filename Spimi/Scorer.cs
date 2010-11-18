using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class Scorer
    {
        private IndexMetadata indexMetadata;
        private IIndex index;

        private const double k = 2.0;     // term frequency scaler
        private const double b = 0.75;   // document length scaler
        private double Lavg;

        public Scorer(IIndex index, IndexMetadata indexMetadata)
        {
            this.index = index;
            this.indexMetadata = indexMetadata;

            this.Lavg = indexMetadata.CollectionLengthInTokens / indexMetadata.CollectionLengthInDocuments;
        }

        public double GetQueryTermScoreContributionForDocument(string docId, string term)
        {
            PostingList postingList = index.GetPostingList(term);

            long N = indexMetadata.CollectionLengthInDocuments;
            int df = postingList.Postings.Count;
            double idf = Math.Log10(N / df);

            int tf = postingList.Postings.Where(p => p.DocumentId == docId).Single().Frequency;
            int Ld = indexMetadata.DocumentLengthMap[docId];
            return idf * (k + 1) * tf / (tf + k * (1 - b + b * Ld / Lavg));
        }
    }
}
