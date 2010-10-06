using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Concordia.Spimi
{
    class SpimiIndexer
    {
        static int maxPostingCountPerBlock = 10000; 

        ILexer lexer;

        List<string> blockFilePaths = new List<string>();

        SpimiBlockWriter blockWriter;

        SpimiBlockReader blockReader;

        FileIndexWriter fileIndexWriter;

        public SpimiIndexer(ILexer lexer)
        {
            this.lexer = lexer;
            this.blockReader = new SpimiBlockReader();
            this.blockWriter = new SpimiBlockWriter();
            this.fileIndexWriter = new FileIndexWriter();
        }

        public void Index(string docId, Stream document)
        {
            foreach (string term in lexer.tokenize(document))
            {
                blockWriter.AddPosting(term, docId);
                if (blockWriter.Postings == maxPostingCountPerBlock)
                {
                    this.FlushBlockWriter();
                }
            }
        }

        void FlushBlockWriter()
        {
            string blockFilePath = blockWriter.FlushToFile();
            blockFilePaths.Add(blockFilePath);
        }

        public void CreateIndex(Stream stream)
        {
            if (blockWriter.Postings > 0)
                FlushBlockWriter();
            List<IEnumerator<PostingList>> openedBlocks = blockReader.OpenBlocks(this.blockFilePaths);
            fileIndexWriter.Write(stream,  blockReader.BeginBlockMerge(openedBlocks));
        }
    }
}
