using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    class ReutersReader
    {
        private ReutersParser parser;
        private  IndexMetadata metadata;
        private string directory;
        
        public ReutersReader(string directory, ReutersParser parser, IndexMetadata metadata)
        {
            this.directory = directory;
            this.parser = parser;
            this.metadata = metadata;
        }

        public string GetDocument(string docID)
        {
            string filepath = metadata.FilePathForDocId(docID);
            FileInfo file = new FileInfo(directory + "\\" + filepath);
            FileStream stream = file.Open(FileMode.Open);
            foreach (Document document in parser.ExtractDocuments(stream))
            {
                if (document.DocId == docID)
                {
                    return document.Body.Replace("     ", "\n");
                }
            }
            return "";
        }
    }
}
