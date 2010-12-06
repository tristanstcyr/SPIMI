

using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using Concordia.Spimi;
using System.Collections.Generic;
using Clusteroo.Models;

public class Spimi
{
    string directory = @"C:\Users\tristan\Downloads\";
    string indexFilePath = @"C:\Users\tristan\Downloads\index";
    string metadataFilePath = @"C:\Users\tristan\Downloads\metadataindex";

    public IndexingStats Index(string site)
    {
        IndexingStats result = new IndexingStats();

        DirectoryInfo directoryInfo = new DirectoryInfo(directory + site);
        if (!directoryInfo.Exists)
        {
            return result;
        }

        DateTime start = DateTime.Now;
        using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.Create))
        {
            using (FileStream metadataFileStream = File.Open(metadataFilePath, FileMode.Create))
            {
                // Index the corpus
                SpimiIndexer indexer = new SpimiIndexer(
                    new BasicLexer(), 
                    new HtmlParser(), 
                    indexFileStream, 
                    metadataFileStream);

                WebCrawler crawler = new WebCrawler(directoryInfo);
                foreach (WebDocument doc in crawler.GetDocuments())
                {
                    Stream stream = doc.Open();
                    indexer.Index(doc.Uri, stream);
                    stream.Close();
                }

                indexer.WriteOut();
                IndexMetadata indexMetadata = new IndexMetadata(metadataFileStream);
                result.CollectionSize = indexMetadata.CollectionLengthInDocuments;
            }
        }
        DateTime end = DateTime.Now;
        result.IndexingTime = (end - start).TotalMilliseconds;
        return result;
    }

    public IList<QueryResult> Query(string query)
    {
        using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.Open))
        {
            using (FileStream metadataFileStream = File.Open(metadataFilePath, FileMode.Open))
            {
                IndexMetadata indexMetadata = new IndexMetadata(metadataFileStream);
                TermIndex index = new TermIndex(indexFileStream);
                QueryEngine queryEngine = new QueryEngine(index, indexMetadata);

                IList<long> results = queryEngine.Query(query.ToLower());
                IList<QueryResult> queryResults = new List<QueryResult>();
                
                int i = 1;
                Console.WriteLine("rank\trsv score\ttitle");
                foreach (long docId in results.Take(500))
                {
                    DocumentInfo docInfo;
                    if (indexMetadata.TryGetDocumentInfo(docId, out docInfo))
                    {
                        QueryResult res = new QueryResult()
                        {
                            Title = docInfo.Title,
                            Uri = docInfo.Uri,
                            Score = queryEngine.Scores[docId]
                        };
                        queryResults.Add(res);
                    }
                    else
                    {
                        Console.WriteLine("Found document id in posting list that wasn't indexed in metadata: " + docId);
                    }
                }

                return queryResults;
            }
        }
    }

    public IList<ClusterResult> Cluster(int k)
    {
        using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.Open))
        {
            using (FileStream metadataFileStream = File.Open(metadataFilePath, FileMode.Open))
            {
                IndexMetadata indexMetadata = new IndexMetadata(metadataFileStream);
                TermIndex index = new TermIndex(indexFileStream);
                QueryEngine queryEngine = new QueryEngine(index, indexMetadata);

                KMeansClusterFinder clusterFinder = new KMeansClusterFinder(indexMetadata, index);
                IList<long> allDocIds = indexMetadata.GetDocumentIds();
                long[][] clusters = clusterFinder.Cluster(allDocIds, k);

                IList<ClusterResult> clusterResults = new List<ClusterResult>();

                foreach (long[] cluster in clusters)
                {
                    // Get the term frequencies in the collection
                    IEnumerable<DocumentInfo> clusterDocuments = indexMetadata.GetDocuments(cluster);
                    TermVector sum = new TermVector();
                    foreach (TermVector vector in clusterDocuments.Select(d => d.TermVector))
                    {
                        sum += vector;
                    }

                    IEnumerable<string> topTerms =
                        TermVector.GetCentroid(indexMetadata.GetDocuments(cluster)
                            .Select(docInfo => docInfo.TermVector))
                        .GetNonZeroDimensions()
                        .OrderByDescending(term => sum.GetDimensionLength(term) * this.GetIdf(index, indexMetadata, term))
                        .Take(6);
                   
                    clusterResults.Add(new ClusterResult(topTerms.ToList(), 
                        clusterDocuments.Select(docInfo => docInfo.Uri).ToList()));
                }

                return clusterResults;
            }
        }
    }

    private double GetIdf(TermIndex index, IndexMetadata metadata, string term)
    {
        double idf = Math.Log(((double)metadata.CollectionLengthInDocuments) / index[term].Count);
        return idf;
    }
}
