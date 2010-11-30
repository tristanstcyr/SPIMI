using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

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


            using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.Create))
            {
                using (FileStream metadataFileStream = File.Open(metadataFilePath, FileMode.Create))
                {
                    // Index the corpus
                    Console.WriteLine("Parsing corpus and creating index blocks...");
                    SpimiIndexer indexer = new SpimiIndexer(
                        new BasicLexer(), 
                        new ReutersParser(), 
                        indexFileStream, 
                        metadataFileStream);

                    DirectoryInfo dir = new DirectoryInfo(directory);
                    foreach(FileInfo file in dir.GetFiles().Where(f => f.Extension.Equals(".sgm")))
                        indexer.Index(file.FullName, file.Open(FileMode.Open));

                    // 2- Build the final index
                    Console.WriteLine("Merging blocks into one index...");
                    indexer.WriteOut();

                    IndexMetadata indexMetadata = new IndexMetadata(metadataFileStream);
                    TermIndex index = new TermIndex(indexFileStream);
                    QueryEngine queryEngine = new QueryEngine(index, indexMetadata);
                    ReutersReader reader = new ReutersReader(directory, new ReutersParser(), indexMetadata);

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
                            Console.WriteLine("rank\tspecial id\tfile location\t\trsv score");
                            foreach (long docId in results.Take(25))
                            {
                                DocumentInfo docInfo;
                                if (indexMetadata.TryGetDocumentInfo(docId, out docInfo))
                                {
                                    const int maxLength = 20;
                                    string url;
                                    if (docInfo.Url.Length > maxLength)
                                    {
                                        int lastIndex = docInfo.Url.Length - 1;
                                        int start = lastIndex - maxLength + 3;
                                        url = "..." + docInfo.Url.Substring(start, maxLength - 3);
                                    }
                                    else
                                    {
                                        url = docInfo.Url;
                                    }

                                    Console.WriteLine(i + ".\t" + docInfo.Identifier + "\t" + url + "\t" +
                                        queryEngine.Scores[docId].ToString().Substring(0, 4));
                                    i++;
                                }
                                else
                                {
                                    Console.WriteLine("Found document id in posting list that wasn't indexedin metadata: "+docId);
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
                            string[] tokens = reader.GetDocument(viewDocID).Split(delimiters);
                            Console.WriteLine();
                            Console.WriteLine("Fetching document...");
                            Console.WriteLine();


                            bool isFirst = true;
                            foreach (string token in tokens)
                            {
                                if (queryTokens.Contains(token.ToLower()))
                                {
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                }
                                else
                                {
                                    Console.ForegroundColor = originalColor;
                                }

                                if (isFirst)
                                    isFirst = false;
                                else
                                    Console.Write(" ");
                                Console.Write(token);
                            }
                            Console.ForegroundColor = originalColor;
                        }

                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
