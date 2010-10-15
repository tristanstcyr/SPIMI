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

        SpimiBlockWriter blockWriter;

        SpimiBlockReader blockReader;

        FileIndexWriter fileIndexWriter;

        DocumentIndex docIndex;

        public SpimiIndexer(ILexer lexer, IParser parser, DocumentIndex docIndex)
        {
            this.lexer = lexer;
            this.parser = parser;
            this.blockReader = new SpimiBlockReader();
            this.blockWriter = new SpimiBlockWriter();
            this.fileIndexWriter = new FileIndexWriter();
            this.docIndex = docIndex;
        }

        public void CreateIndexBlocks(string filePath, Stream file)
        {
            // Each file holds many documents: we need to parse them out first.
            foreach (Document document in parser.scrub(file))
            {
                // Keep track of in which file each document is
                //docIndex.FilePaths.Add(document.DocId, filePath);

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
    }
}
