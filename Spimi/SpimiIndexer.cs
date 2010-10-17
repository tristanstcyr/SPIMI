using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Concordia.Spimi
{
    class SpimiIndexer
    {
        static int maxPostingCountPerBlock = 10000; 

        ILexer lexer;

        IParser parser;

        List<string> blockFilePaths = new List<string>();

        Dictionary<int, string> documentMap = new Dictionary<int, string>();

        SpimiBlockWriter blockWriter;

        SpimiBlockReader blockReader;

        FileIndexWriter fileIndexWriter;
        
        public SpimiIndexer(ILexer lexer, IParser parser)
        {
            this.lexer = lexer;
            this.parser = parser;
            this.blockReader = new SpimiBlockReader();
            this.blockWriter = new SpimiBlockWriter();
            this.fileIndexWriter = new FileIndexWriter();
        }

        public void Index(string filePath, Stream file)
        {
            bool gotFirstDocIdInFile = false;
            // Each file holds many documents: we need to parse them out first.
            foreach (Document document in parser.ExtractDocuments(file))
            {
                if (!gotFirstDocIdInFile)
                {
                    // Keep track of in which file what the start docId is for
                    // easier query result retrieval
                    documentMap.Add(int.Parse(document.DocId), filePath);
                    gotFirstDocIdInFile = true;
                }

                // Extract the terms from the document
                foreach (string term in lexer.tokenize(document.Body))
                {
                    blockWriter.AddPosting(term, document.DocId);
                    if (blockWriter.Postings == maxPostingCountPerBlock)
                    {
                        this.FlushBlockWriter();
                    }
                }
            }
        }

        void FlushBlockWriter()
        {
            string blockFilePath = blockWriter.FlushToFile();
            blockFilePaths.Add(blockFilePath);
        }

        public void MergeIndexBlocks(Stream stream)
        {
            if (blockWriter.Postings > 0)
                FlushBlockWriter();
            List<IEnumerator<PostingList>> openedBlocks = blockReader.OpenBlocks(this.blockFilePaths);
            fileIndexWriter.Write(stream,  blockReader.BeginBlockMerge(openedBlocks));
        }

        public string FilePathForDocId(string docId)
        {
            int docIdInt = int.Parse(docId);
            int lastDocId = 1;
            foreach(int key in documentMap.Keys.OrderBy(k => k))
            {
                if (key > docIdInt)
                    return documentMap[lastDocId];
                else if (key == docIdInt)
                    return documentMap[key];
                lastDocId = key;
            }
            return documentMap[lastDocId];
        }
    }
}
