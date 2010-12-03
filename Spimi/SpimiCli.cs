using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
                    Console.WriteLine("Done! Please query the corpus:");
                    
                    IList<long> results = null;
                    string query = null;

                    while (true)
                    {
                        Console.Write("> ");
                        string input = Console.ReadLine();
                    
                        int selectedRank = 0;
                        if (!int.TryParse(input, out selectedRank))
                        {
                            query = input.ToLower();
                            results = queryEngine.Query(input.ToLower());
                            
                            // Print the ranked results
                            int i = 1;
                            Console.WriteLine("rank\tspecial id\trsv score\ttitle");
                            foreach (long docId in results.Take(25))
                            {
                                DocumentInfo docInfo;
                                if (indexMetadata.TryGetDocumentInfo(docId, out docInfo))
                                {
                                    const int maxLength = 30;
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
                                    Console.WriteLine(i + ".\t" + docInfo.SpecialIdentifier + "\t" + score + "\t" + title);
                                    i++;
                                }
                                else
                                {
                                    Console.WriteLine("Found document id in posting list that wasn't indexed in metadata: "+docId);
                                }
                            }
                            Console.WriteLine(results.Count + " hit(s). (enter hit rank number to read entry, or query again)");
                        }
                        else if (selectedRank > 0 && results != null && results.Count >= selectedRank)
                        {
                            // Fetch the document from the collection and display it
                            ConsoleColor originalColor = Console.ForegroundColor;
                            char[] delimiters = { ' ', '.', '\t', '\n', ',', ';', ':'};
                            string[] queryTokens = query.Split(delimiters);
                            long viewDocID = results[selectedRank-1];

                            DocumentInfo docInfo;
                            indexMetadata.TryGetDocumentInfo(viewDocID, out docInfo);

                            Process.Start(docInfo.Uri);
                        }

                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
