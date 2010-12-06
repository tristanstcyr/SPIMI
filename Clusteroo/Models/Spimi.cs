

using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using Concordia.Spimi;
using System.Collections.Generic;
using Clusteroo.Models;

public class Spimi
{
    string directory = @"C:\Users\tristan\Downloads\www.arstechnica.com";
    string indexFilePath = @"C:\Users\tristan\Downloads\index";
    string metadataFilePath = @"C:\Users\tristan\Downloads\metadataindex";

    public void Index()
    {
        
        Console.WriteLine("Welcome to Spimi!");

        DirectoryInfo directoryInfo = new DirectoryInfo(directory);
        if (!directoryInfo.Exists)
        {
            Console.WriteLine("Directory could not be found");
            return;
        }

        using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.Create))
        {
            using (FileStream metadataFileStream = File.Open(metadataFilePath, FileMode.Create))
            {
                // Index the corpus
                Console.WriteLine("Parsing corpus and creating index blocks...");
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

                // 2- Build the final index
                Console.WriteLine("Merging blocks into one index...");
                indexer.WriteOut();

                

                // 3- Query the index
                Console.WriteLine("Done! Please use one of the following commands: \n/query <term1> <term2>\n/cluster <k>\n");
            }
        }
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
                foreach (long docId in results.Take(25))
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
                        sum += vector;

                    // Order terms in the collection by tf-idf
                    IEnumerable<string> topTerms = 
                        TermVector.GetCentroid(clusterDocuments.Select(
                            docInfo => docInfo.TermVector))
                        .GetNonZeroDimensions()
                        .OrderBy(term => sum.GetDimensionLength(term)*this.GetIdf(index, indexMetadata, term))
                        .Take(10);
                   
                    clusterResults.Add(new ClusterResult(topTerms.ToList(), 
                        clusterDocuments.Select(docInfo => docInfo.Uri).ToList()));
                }

                return clusterResults;
            }
        }
    }

    private double GetIdf(TermIndex index, IndexMetadata metadata, string term)
    {
        return ((double)metadata.CollectionLengthInDocuments) / index[term].Count;
    }
}
