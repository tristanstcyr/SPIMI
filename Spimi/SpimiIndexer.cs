using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Concordia.Spimi
{
    /// <summary>
    /// Core class of the SPIMI project. The SpimiIndexer is in charge
    /// of 1) parsing and tokenizing a corpus, 2) building the dictionary
    /// and posting lists.
    /// </summary>
    class SpimiIndexer
    {
        static int maxPostingCountPerBlock = 10000;

        ILexer lexer;

        IParser parser;

        List<string> blockFilePaths = new List<string>();      

        SpimiBlockWriter blockWriter;

        SpimiBlockReader blockReader;

        FileIndexWriter fileIndexWriter;

        Dictionary<string, string> documentMap = new Dictionary<string, string>();
        Dictionary<string, int> documentLengthMap = new Dictionary<string, int>();
        long collectionLengthInTokens = 0;
        long collectionLengthInDocuments = 0;
               
        public SpimiIndexer(ILexer lexer, IParser parser)
        {
            this.lexer = lexer;
            this.parser = parser;
            this.blockReader = new SpimiBlockReader();
            this.blockWriter = new SpimiBlockWriter();
            this.fileIndexWriter = new FileIndexWriter();

        }

        /// <summary>
        /// Parses and tokenizes the inputted file. New terms are added to the 
        /// dictionary, and reccurent terms are added to those terms' postings list.
        /// </summary>
        /// <param name="filePath">The path of the file to index.</param>
        /// <param name="file">The already opened file stream of the file in question.</param>
        public void Index(string filePath, Stream file)
        {
            bool gotFirstDocIdInFile = false;
            

            // Each file holds many documents: we need to parse them out first.
            foreach (Document document in parser.ExtractDocuments(file))
            {
                if (!gotFirstDocIdInFile)
                {
                    // Keep track of (filePath, firstDocIdInThatFile) pairs for easier query result retrieval
                    documentMap.Add(document.DocId, filePath);
                    gotFirstDocIdInFile = true;
                }

                // Extract the terms from the document and add the document to their respective postings lists
                int termsInDoc = 0;
                foreach (string term in lexer.Tokenize(document.Body))
                {
                    blockWriter.AddPosting(term, document.DocId);
                    if (blockWriter.Postings == maxPostingCountPerBlock)
                    {
                        // 
                        this.FlushBlockWriter();
                    }
                    termsInDoc++;
                    collectionLengthInTokens++;
                }
                documentLengthMap.Add(document.DocId, termsInDoc);
                collectionLengthInDocuments++;
            }
        }

        public IndexMetadata GetMetadata()
        {
            return new IndexMetadata(documentMap, documentLengthMap, collectionLengthInDocuments, collectionLengthInTokens);
        }

        private void FlushBlockWriter()
        {
            string blockFilePath = blockWriter.FlushToFile();
            blockFilePaths.Add(blockFilePath);
        }

        /// <summary>
        /// Once the corpus is fully indexed, call this to merge all the intermediate inverted indexes into a single index.
        /// </summary>
        /// <param name="stream">The opened destination filestream for the full inverted index.</param>
        public void MergeIndexBlocks(Stream stream)
        {
            if (blockWriter.Postings > 0)
                FlushBlockWriter();
            List<IEnumerator<PostingList>> openedBlocks = blockReader.OpenBlocks(this.blockFilePaths);
            fileIndexWriter.Write(stream,  blockReader.BeginBlockMerge(openedBlocks));
        }

    }
}
