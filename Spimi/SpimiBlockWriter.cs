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

        public int Postings 
        {
            get
            {
                return index.Postings;
            }
        }

        public void AddPosting(string term, long docId)
        {
            index.AddTerm(term, docId);
        }

        public string FlushToFile()
        {
            
            string filename = Path.GetTempFileName();
            
            SortedList<string, SortedList<long, Posting>> orderedEntries = index.Entries;

            using (FileStream fs = File.Open(filename, FileMode.Append))
            {
                PostingListEncoder encoder = new PostingListEncoder();
                BinaryWriter writer = new BinaryWriter(fs);

                writer.Write((Int32)orderedEntries.Count);

                foreach (KeyValuePair<string, SortedList<long, Posting>> termPostingsPair in orderedEntries)
                {
                    writer.Write((string)termPostingsPair.Key);
                    encoder.write(writer, termPostingsPair.Value.Values);
                }
            }

            this.index = new MemoryIndex();
            return filename;
        }
    }
}
