using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Concordia.Spimi
{
    /// <summary>
    /// Driver class of the SPIMI project.
    /// </summary>
    public class SpimiCli
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("usage: Spimi <folderpath> <DestinationIndexFilePath> <metadatafilepath");
                Console.ReadLine();
                return;
            }
            string directory = args[0];
            string indexFilePath = args[1];
            string metadataFilePath = args[2];

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

                    IndexMetadata indexMetadata = new IndexMetadata(metadataFileStream);
                    TermIndex index = new TermIndex(indexFileStream);
                    QueryEngine queryEngine = new QueryEngine(index, indexMetadata);

                    // 3- Query the index
                    Console.WriteLine("Done! Please use one of the following commands: \n/query <term1> <term2>\n/cluster <k>\n");

                    QueryCli cli = new QueryCli(indexMetadata, index);
                    cli.Run();
                }
            }
        }

        class QueryCli 
        {

            IndexMetadata metadata;
            
            TermIndex index;

            QueryEngine queryEngine;

            IList<long> results;

            public QueryCli(IndexMetadata metadata, TermIndex index)
            {
                this.metadata = metadata;
                this.index = index;
                this.queryEngine = new QueryEngine(index, metadata);
            }

            public void ProcessQuery(string param)
            {
                results = queryEngine.Query(param.ToLower(), RankingMode.BM25);
                this.PrintResults();
            }

            void PrintResults()
            {
                int i = 1;
                Console.WriteLine("rank\trsv score\ttitle");
                foreach (long docId in results.Take(25))
                {
                    DocumentInfo docInfo;
                    if (metadata.TryGetDocumentInfo(docId, out docInfo))
                    {
                        const int maxLength = 45;
                        string title;
                        if (docInfo.Title.Length > maxLength)
                        {
                            title = docInfo.Title.Substring(0, maxLength) + "...";
                        }
                        else
                        {
                            title = docInfo.Title;
                        }

                        double score = queryEngine.Scores[docId];
                        string shortScore = score.ToString().Length > 4 ? score.ToString().Substring(0, 4) : score.ToString();
                        Console.WriteLine(i + "\t" + shortScore + "\t" + title);
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Found document id in posting list that wasn't indexed in metadata: " + docId);
                    }
                }
                Console.WriteLine(results.Count + " hit(s). (enter \"/show <hit rank number>\" to read entry, or /query again)");
            }

            public void ShowResult(string param)
            {
                int position = 0;
                if (results == null)
                {
                    Console.WriteLine("Please query first");
                    return;
                }
                else if (!int.TryParse(param, out position) || position < 0 || results.Count < position)
                {
                    Console.WriteLine("Must specify a number in the results");
                    return;
                }

                DocumentInfo docInfo;
                metadata.TryGetDocumentInfo(results[position - 1], out docInfo);
                Process.Start(docInfo.Uri);
            }

            public void Run()
            {
                while (true)
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    Match matches = Regex.Match(input, "^/(?<command>[a-z]*) (?<arguments>.*)$");
                    if (matches.Groups.Count < 2)
                    {
                        PrintUsage();
                        continue;
                    }

                    string command = matches.Groups["command"].Value;
                    string args = matches.Groups["arguments"].Value;

                    switch (command)
                    {
                        case "query":
                            this.ProcessQuery(args);
                            break;
                        case "show":
                            this.ShowResult(args);
                            break;
                        case "cluster":
                            this.Cluster(args);
                            break;
                        default:
                            PrintUsage();
                            break;
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            public double GetIdf(string term)
            {
                double idf = Math.Log(((double)this.metadata.CollectionLengthInDocuments) / this.index[term].Count);
                return idf;
            }

            private long ClusterTermFrequency(IEnumerable<long> cluster, string term)
            {
                return this.metadata.GetDocuments(cluster).Sum(d => d.TermVector.GetDimensionLength(term));
            }

            private double ClusterTfIdf(IEnumerable<long> cluster, string term)
            {
                return this.ClusterTermFrequency(cluster, term) * this.GetIdf(term);
            }

            private void Cluster(string args)
            {
                int k = int.Parse(args);
                Console.WriteLine("Clustering...");
                KMeansClusterFinder clusterFinder = new KMeansClusterFinder(this.metadata, this.index);
                IList<long> allDocIds = this.metadata.GetDocumentIds();
                long[][] clusters = clusterFinder.Cluster(allDocIds, k);
                int i = 1;
                Console.WriteLine("Done! Here are top terms for each cluster:");
                
                foreach(long[] cluster in clusters)
                {
                    IEnumerable<DocumentInfo> clusterDocuments = this.metadata.GetDocuments(cluster);
                    TermVector sum = new TermVector();
                    foreach (TermVector vector in clusterDocuments.Select(d => d.TermVector))
                    {
                        sum += vector;
                    }

                    IEnumerable<string> topTerms = 
                        TermVector.GetCentroid(this.metadata.GetDocuments(cluster)
                            .Select(docInfo => docInfo.TermVector))
                        .GetNonZeroDimensions()
                        .OrderByDescending(term => sum.GetDimensionLength(term)*this.GetIdf(term))
                        .Take(6);

                    Console.Write(i + ": ");
                    if (topTerms.Count() == 0)
                    {
                        Console.Write("Empty cluster!");
                        Console.WriteLine();
                    }
                    else
                    {
                        foreach (string term in topTerms)
                        {
                            Console.Write(term+"  ");
                        }
                        Console.WriteLine();
                    }
                    i++;
                }
            }

            void PrintUsage()
            {
                Console.WriteLine("usage: /<command> <arguments>");

            }
        }
    }
}
