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

        List<string> termIndexBlockFilePaths = new List<string>();

        List<string> documentIndexBlockFilePaths = new List<string>();

        SpimiBlockWriter termIndexBlockWriter;

        CollectionMetadataWriter metadataWriter;

        long collectionLengthInTokens = 0;

        long nextDocumentId = 0;

        Stream indexStream;

        public SpimiIndexer(ILexer lexer, IParser parser, Stream indexStream, Stream metadata)
        {
            this.lexer = lexer;
            this.parser = parser;
            this.termIndexBlockWriter = new SpimiBlockWriter();
            this.indexStream = indexStream;
            this.metadataWriter = new CollectionMetadataWriter(metadata);
        }

        /// <summary>
        /// Parses and tokenizes the inputted file. New terms are added to the 
        /// dictionary, and reccurent terms are added to those terms' postings list.
        /// </summary>
        /// <param name="uri">The path of the file to index.</param>
        /// <param name="file">The already opened file stream of the file in question.</param>
        public void Index(string uri, Stream file)
        {
            // Each file holds many documents: we need to parse them out first.
            foreach (Document document in parser.ExtractDocuments(file))
            {
                // Extract the terms from the document and add the document to their respective postings lists
                long docId = nextDocumentId++;
                int termsInDoc = 0;
                IEnumerable<string> terms = lexer.Tokenize(document.Body);
                TermVector vector = new TermVector();
                foreach (string term in terms)
                {
                    vector.AddTerm(term);

                    termIndexBlockWriter.AddPosting(term, docId);
                    if (termIndexBlockWriter.Postings == maxPostingCountPerBlock)
                    {
                        // 
                        this.FlushBlockWriter();
                    }
                    termsInDoc++;
                    collectionLengthInTokens++;
                }

                this.metadataWriter.AddDocumentInfo(docId,
                    new DocumentInfo(uri, document.Title, termsInDoc, document.SpecialIdentifier, vector));
            }
        }

        private void FlushBlockWriter()
        {
            string blockFilePath = termIndexBlockWriter.FlushToFile();
            termIndexBlockFilePaths.Add(blockFilePath);
        }

        /// <summary>
        /// Once the corpus is fully indexed, call this to merge all the intermediate inverted indexes into a single index.
        /// </summary>
        /// <param name="stream">The opened destination filestream for the full inverted index.</param>
        public void WriteOut()
        {
            MergeBlocks();
            this.metadataWriter.WriteOut();
        }

        private void MergeBlocks()
        {
            if (termIndexBlockWriter.Postings > 0)
                FlushBlockWriter();
            using (FileIndexWriter<string, IList<Posting>> writer = new FileIndexWriter<string, IList<Posting>>(
                new StringEncoder(),
                new PostingListEncoder(), indexStream))
            {
                SpimiBlockReader blockReader = new SpimiBlockReader();
                List<IEnumerator<PostingList>> openedBlocks = blockReader.OpenBlocks(this.termIndexBlockFilePaths);
                foreach (PostingList postingList in blockReader.BeginBlockMerge(openedBlocks))
                {
                    writer.Add(postingList.Term, postingList.Postings);
                }
                writer.WriteOut();
            }
        }

    }
}
