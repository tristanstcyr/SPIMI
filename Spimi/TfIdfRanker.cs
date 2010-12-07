using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class TfIdfRanker : Concordia.Spimi.IRanker
    {
        private TermIndex index;
        private IndexMetadata indexMetadata;
        private TfIdfScorer scorer;

        public TfIdfRanker(TermIndex index, IndexMetadata indexMetadata)
        {
            this.index = index;
            this.indexMetadata = indexMetadata;
            this.scorer = new TfIdfScorer(index, indexMetadata);
        }

        public IList<long> Rank(string[] terms, IEnumerable<long> hits)
        {
            scores.Clear();
            return hits.OrderByDescending(hit => GetDocScoreForQuery(hit, terms)).ToList();
        }

        private Dictionary<long, double> scores = new Dictionary<long, double>();

        public Dictionary<long, double> Scores {
            get { return scores; }
        }

        private double GetDocScoreForQuery(long docId, string[] terms)
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
