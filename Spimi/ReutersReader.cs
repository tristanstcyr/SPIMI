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

        public string GetDocument(long docID)
        {
            DocumentInfo docInfo;
            if (!metadata.TryGetDocumentInfo(docID, out docInfo))
                throw new InvalidOperationException("docId "+docID+" was not found in the metadata");

            FileInfo file = new FileInfo(docInfo.Url);
            FileStream stream = file.Open(FileMode.Open);
            foreach (Document document in parser.ExtractDocuments(stream))
            {
                if (document.DocId.Equals(docInfo.Identifier))
                {
                    return document.Body.Replace("     ", "\n");
                }
            }
            return "";
        }
    }
}
