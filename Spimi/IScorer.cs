using System;
namespace Concordia.Spimi
{
    public interface IScorer
    {
        double GetQueryTermScoreContributionForDocument(long docId, string term);
    }
}
