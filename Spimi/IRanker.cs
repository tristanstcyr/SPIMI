using System;
namespace Concordia.Spimi
{
    public interface IRanker
    {
        System.Collections.Generic.IList<long> Rank(string[] terms, System.Collections.Generic.IEnumerable<long> hits);
        System.Collections.Generic.Dictionary<long, double> Scores { get; }
    }
}
