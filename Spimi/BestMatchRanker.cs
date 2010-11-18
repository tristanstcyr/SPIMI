using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class BestMatchRanker
    {
        private IIndex index;
        private IndexMetadata indexMetadata;
        private Scorer scorer;

        public BestMatchRanker(IIndex index, IndexMetadata indexMetadata)
        {
            this.index = index;
            this.indexMetadata = indexMetadata;
            this.scorer = new Scorer(index, indexMetadata);
        }

        public IList<Posting> Rank(string[] terms, IList<Posting> hits)
        {
            scores.Clear();
            return hits.OrderByDescending(posting => GetDocScoreForQuery(posting.DocumentId, terms)).ToList();
        }

        private Dictionary<string, double> scores = new Dictionary<string, double>();

        public Dictionary<string, double> Scores {
            get { return scores; }
        }

        private double GetDocScoreForQuery(string docId, string[] terms)
        {
            double score = 0.0;
            foreach (string term in terms)
            {
                score += scorer.GetQueryTermScoreContributionForDocument(docId, term);
            }
            scores.Add(docId, score);
            return score;
        }
    }
}
