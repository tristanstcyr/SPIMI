using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Concordia.Spimi
{
    public class KMeansClusterFinder
    {
        IndexMetadata metadata;
        TermIndex index;

        public KMeansClusterFinder(IndexMetadata metadata, TermIndex index)
        {
            this.metadata = metadata;
            this.index = index;
        }

        public long[][] Cluster(IList<long> documentIds, int k)
        {
            TermVector[] centroids = GetRandomCentroidsFromDocuments(documentIds, k);
            
            // Loop until we converge or reach 9 iterations
            List<long>[] bestCluster = null;
            double bestRss;
            List<long>[] latestCluster = new List<long>[k];
            double newRss = double.MaxValue;
            int iterationCount = 0;
            do
            {
                bestRss = newRss;
                bestCluster = latestCluster;
                latestCluster = new List<long>[k];
                         
                this.Cluster(documentIds, centroids, latestCluster);
                centroids = this.CalculateCentroids(latestCluster);
                newRss = this.CalculateRss(latestCluster);
                iterationCount++;
            }
            while (bestRss > newRss || iterationCount == 9);

            // Convert to fixed arrays
            int clusterIndex = 0;
            long[][] clusters = new long[k][];
            foreach (List<long> cluster in bestCluster)
            {
                clusters[clusterIndex] = bestCluster[clusterIndex].ToArray();
                clusterIndex++;
            }

            return clusters;
        }

        TermVector[] CalculateCentroids(List<long>[] clusters)
        {
            TermVector[] centoids = new TermVector[clusters.Length];
            int clusterIndex = 0;
            foreach (List<long> cluster in clusters)
            {
                centoids[clusterIndex] = TermVector.GetCentroid(
                    this.GetTermVectors(this.metadata.GetDocuments(cluster)));
                clusterIndex++;
            }

            return centoids;
        }

        private double CalculateRss(IEnumerable<IEnumerable<long>> clusters)
        {
            double totalRss = 0;
            foreach (IEnumerable<long> cluster in clusters)
            {
                double clusterRss = 0;
                IEnumerable<DocumentInfo> documentInfos = metadata.GetDocuments(cluster);
                TermVector centroid = TermVector.GetCentroid(GetTermVectors(documentInfos));
                foreach (TermVector vector in GetTermVectors(documentInfos))
                {
                    double differenceLength = (vector - centroid).EuclideanLength();
                    clusterRss += differenceLength * differenceLength;
                }

                totalRss += clusterRss;
            }

            return totalRss;
        }

        private IEnumerable<TermVector> GetTermVectors(IEnumerable<DocumentInfo> docs)
        {
            foreach (DocumentInfo doc in docs)
                yield return doc.TermVector;
        }

        private void Cluster(IEnumerable<long> documentsToCluster, TermVector[] centroids, List<long>[] clusters)
        {
            Contract.Requires(centroids.Length == clusters.Length);

            // Init clusters
            for (int clusterIndex = 0; clusterIndex < centroids.Length; clusterIndex++)
                clusters[clusterIndex] = new List<long>();

            // Assign documents to clusters
            foreach (long documentId in documentsToCluster)
            {
                // Find nearest centroid
                TermVector termVector = metadata[documentId].TermVector;

                int clusterIndex = 0;
                double minDistance = double.MaxValue;
                int minDistanceIndex = 0;
                foreach (TermVector centroid in centroids)
                {
                    //double distance = termVector.CosineSimilarity(centroid);
                    double distance = (centroid - termVector).EuclideanLength();
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minDistanceIndex = clusterIndex;
                    }
                    clusterIndex++;
                }

                clusters[minDistanceIndex].Add(documentId);
            }
        }

        private TermVector[] GetRandomCentroidsFromDocuments(IList<long> documentsToCluster, int k)
        {
            HashSet<long> seeds = new HashSet<long>();
            TermVector[] centroids = new TermVector[k];
            Random random = new Random();

            // Start k with random seeds
            for (int seedIndex = 0; seedIndex < k; seedIndex++)
            {
                long seedDocId;

                do
                {
                    seedDocId = documentsToCluster[random.Next(0, (int)documentsToCluster.Count)];
                }
                while (seeds.Contains(seedDocId));
                centroids[seedIndex] = metadata[seedDocId].TermVector;
            }

            return centroids;
        }
    }
}
