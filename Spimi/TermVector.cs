using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Concordia.Spimi
{
    public class TermVector
    {
        Dictionary<string, int> vector;

        public TermVector() 
        {
            this.vector = new Dictionary<string, int>();
        }

        public ICollection<string> Terms
        {
            get
            {
                return this.vector.Keys;
            }
        }

        public int GetDimensionLength(string termDimension)
        {
            int value;
            vector.TryGetValue(termDimension, out value);
            return value;
        }

        public TermVector(Dictionary<string, int> vector)
        {
            this.vector = vector;
        }

        public void AddTerm(string term)
        {
            int count = this.GetDimensionLength(term);
            vector.Remove(term);
            vector.Add(term, count + 1);
        }

        public double CosineSimilarity(TermVector centroid)
        {
            return this.DotProduct(centroid) / (this.EuclideanLength() * centroid.EuclideanLength());

        }

        public int DotProduct(TermVector other)
        {
            int sum = 0;
            foreach (string term in this.Terms)
                sum += this.GetDimensionLength(term) * other.GetDimensionLength(term);
            return sum;
        }

        public double EuclideanLength()
        {
            double sum = 0;
            foreach (int length in vector.Values)
                sum += length * length;
            return Math.Sqrt(sum);
        }

        public static TermVector operator-(TermVector left, TermVector right)
        {
            Dictionary<string, int> difference = new Dictionary<string, int>(left.vector);
            foreach (string term in right.Terms)
            {
                int length = 0;
                if (difference.TryGetValue(term, out length))
                    difference.Remove(term);
                difference.Add(term, length - right.GetDimensionLength(term));
            }

            return new TermVector(difference);
        }

        public static TermVector operator +(TermVector left, TermVector right)
        {
            Dictionary<string, int> sum = new Dictionary<string, int>(left.vector);
            foreach (string term in right.Terms)
            {
                int length = 0;
                if (sum.TryGetValue(term, out length))
                    sum.Remove(term);
                sum.Add(term, length + right.GetDimensionLength(term));
            }

            return new TermVector(sum);
        }

        public IEnumerable<string> GetLengthSortedDimensions()
        {
            return this.vector.OrderBy(pair => pair.Value).Select(p => p.Key);
        }

        public IEnumerable<string> GetNonZeroDimensions()
        {
            return this.vector.Select(p => p.Key);
        }

        public static TermVector GetCentroid(IEnumerable<TermVector> vectors)
        {
            Dictionary<string, long> sum = new Dictionary<string, long>();

            int vectorCount = 0;
            // Sum the lengths of dimensions
            foreach (TermVector vector in vectors)
            {
                vectorCount++;
                foreach (string term in vector.Terms)
                {
                    long count = 0;
                    sum.TryGetValue(term, out count);
                    sum.Remove(term);
                    sum.Add(term, count + vector.GetDimensionLength(term));
                }
            }

            // Divide the dimensions
            Dictionary<string, int> centroid = new Dictionary<string, int>();
            foreach (KeyValuePair<string, long> dimension in sum)
            {
                centroid.Add(dimension.Key, (int)(dimension.Value / vectorCount));
            }

            return new TermVector(centroid);
        }
    }
}
