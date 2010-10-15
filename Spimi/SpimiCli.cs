using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Concordia.Spimi
{
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
            Console.WriteLine("Parsing corpus and creating index blocks..");
            DocumentIndex docIndex = new DocumentIndex();
            SpimiIndexer indexer = new SpimiIndexer(new BasicLexer(), new ArticleParser(), docIndex);
            
            DirectoryInfo dir = new DirectoryInfo(directory);
            foreach(FileInfo file in dir.GetFiles().Where(f => f.Extension.Equals(".sgm")))
                indexer.CreateIndexBlocks(file.FullName, file.Open(FileMode.Open));
            
            using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.Create))
            {
                Console.WriteLine("Merging blocks into one index..");
                indexer.MergeIndexBlocks(indexFileStream);

                FileIndex index = FileIndex.Open(indexFileStream);
                QueryEngine queryEngine = new QueryEngine(index);
                Console.WriteLine("Done! Please query the corpus:");
                while (true)
                {
                    Console.Write("> ");
                    string query = Console.ReadLine();
                    foreach (string docId in queryEngine.Query(query.ToLower()))
                    {
                        Console.WriteLine(docId /*+ " in " + docIndex.FilePaths[docId]*/);
                    }
                }
            }
        }
    }
}
