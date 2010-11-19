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
            if (args.Length != 2)
            {
                Console.WriteLine("usage: Spimi <folderpath> <DestinationIndexFilePath>");
                Console.ReadLine();
                return;
            }
            string directory = args[0];
            string indexFilePath = args[1];

            Console.WriteLine("Welcome to Spimi!");

            // 1- Index the corpus
            Console.WriteLine("Parsing corpus and creating index blocks...");
            ReutersParser parser = new ReutersParser();
            SpimiIndexer indexer = new SpimiIndexer(new BasicLexer(), parser);
                        
            DirectoryInfo dir = new DirectoryInfo(directory);
            foreach(FileInfo file in dir.GetFiles().Where(f => f.Extension.Equals(".sgm")))
                indexer.Index(file.Name, file.Open(FileMode.Open));
            IndexMetadata indexMetadata = indexer.GetMetadata();
            
            using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.Create))
            {
                // 2- Build the final index
                Console.WriteLine("Merging blocks into one index...");
                indexer.MergeIndexBlocks(indexFileStream);

                // 3- Query the index
                FileIndex index = FileIndex.Open(indexFileStream);
                QueryEngine queryEngine = new QueryEngine(index, indexMetadata);
                ReutersReader reader = new ReutersReader(directory, parser, indexMetadata);
                Console.WriteLine("Done! Please query the corpus:");
                IList<Posting> results = null;
                while (true)
                {
                    Console.Write("> ");
                    string query = Console.ReadLine();
                    int selectedRank = 0;
                    if (!int.TryParse(query, out selectedRank))
                    {
                        results = queryEngine.Query(query.ToLower());

                        // Print the ranked results
                        int i = 1;
                        Console.WriteLine("rank\tdoc id\tfile location\trsv score");
                        foreach (Posting posting in results.Take(25))
                        {
                            Console.WriteLine(i + ".\t" + posting.DocumentId + "\t" + indexMetadata.FilePathForDocId(posting.DocumentId) + "\t" + queryEngine.Scores[posting.DocumentId].ToString().Substring(0, 4));
                            i++;
                        }
                        Console.WriteLine(results.Count + " hit(s). (enter hit rank number to read entry, or query again)\n");
                    }
                    else if (selectedRank > 0 && results != null && results.Count >= selectedRank)
                    {
                        // Fetch the document from the collection and display it
                        string viewDocID = results[selectedRank-1].DocumentId;
                        Console.WriteLine("Fetching document " + viewDocID + "...");
                        Console.WriteLine(reader.GetDocument(viewDocID));
                        Console.WriteLine();                                                                         
                    }
                }
            }
        }
    }
}
