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
                    documentMap.Add(int.Parse(document.DocId), filePath);
                    gotFirstDocIdInFile = true;
                }

                // Extract the terms from the document and add the document to their respective postings lists
                foreach (string term in lexer.Tokenize(document.Body))
                {
                    blockWriter.AddPosting(term, document.DocId);
                    if (blockWriter.Postings == maxPostingCountPerBlock)
                    {
                        // 
                        this.FlushBlockWriter();
                    }
                }
            }
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

        /// <summary>
        /// Helper method that returns the file in which the specified document can be found.
        /// </summary>
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
