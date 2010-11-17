using System.Collections.Generic;
using System.IO;
using System.Collections;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Concordia.Spimi
{
    /// <summary>
    /// Specialist class in charge of creating and storing the intermediate inverted index blocks.
    /// </summary>
    class SpimiBlockWriter
    {
        MemoryIndex index = new MemoryIndex();

        public long SerializedTermsSize { get; private set; }

        public int Postings 
        {
            get
            {
                return index.Postings;
            }
        }

        public SpimiBlockWriter()
        {
            SerializedTermsSize = 0;
        }

        public void AddPosting(string term, string docId)
        {
            index.AddTerm(term, docId);
        }

        public string FlushToFile()
        {
            string filename = Path.GetTempFileName();
            IOrderedEnumerable<string> orderedEntries = index.Vocabulary.OrderBy(term => term);

            using (FileStream fs = File.Open(filename, FileMode.Append))
            {
                BinaryWriter writer = new BinaryWriter(fs);

                writer.Write((Int32)orderedEntries.Count());

                foreach (string term in orderedEntries)
                {
                    IList<Posting> postings = index.GetPostingList(term).Postings;

                    // term
                    long termLocation = fs.Position;
                    writer.Write((string)term);
                    SerializedTermsSize += fs.Position - termLocation;
                    
                    // number of postings
                    writer.Write((Int32)postings.Count);
                    
                    // postings
                    foreach (Posting posting in postings)
                    {
                        writer.Write((string)posting.DocumentId);
                        writer.Write((Int32)posting.Frequency);
                    }
                }
            }

            this.index = new MemoryIndex();
            return filename;
        }
    }
}
