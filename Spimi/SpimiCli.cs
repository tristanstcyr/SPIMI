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
                return;
            }
            string directory = args[0];
            string indexFilePath = args[1];
            SpimiIndexer indexer = new SpimiIndexer(new BasicLexer());
            
            DirectoryInfo dir = new DirectoryInfo(directory);
            foreach(FileInfo file in dir.GetFiles().Where(f => f.Extension.Equals(".sgm")))
                indexer.Index(file.FullName, file.Open(FileMode.Open));
            
            using (FileStream indexFileStream = File.Open(indexFilePath, FileMode.CreateNew))
            {
                indexer.CreateIndex(indexFileStream);
                FileIndex index = FileIndex.Open(indexFileStream);
                QueryEngine queryEngine = new QueryEngine(index);
                while (true)
                {
                    Console.Write("> ");
                    string query = Console.ReadLine();
                    foreach (string docId in queryEngine.Query(query.ToLower()))
                    {
                        Console.WriteLine(docId);
                    }
                }
            }
        }
    }
}
