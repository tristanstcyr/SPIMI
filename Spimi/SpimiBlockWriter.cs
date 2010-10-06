using System.Collections.Generic;
using System.IO;
using System.Collections;
using System;
using System.Linq;

namespace Concordia.Spimi
{
    class SpimiBlockWriter
    {
        Dictionary<string, HashSet<string>> postingLists = new Dictionary<string, HashSet<string>>();

        public long SerializedTermsSize { get; private set; }

        public int Postings { get; private set; }

        public SpimiBlockWriter()
        {
            SerializedTermsSize = 0;
            Postings = 0;
        }

        public void AddPosting(string term, string docId)
        {
            HashSet<string> postingList;
            if (!postingLists.TryGetValue(term, out postingList))
            {
                postingList = new HashSet<string>();
                postingLists.Add(term, postingList);
            }
            postingList.Add(docId);
            Postings++;
        }

        public string FlushToFile()
        {
            string filename = Path.GetTempFileName();
            using (FileStream fs = File.Open(filename, FileMode.Append))
            {
                BinaryWriter writer = new BinaryWriter(fs);
                writer.Write((Int32)postingLists.Count);
                var orderedEntries = this.postingLists.OrderBy(e => e.Key);
                foreach (KeyValuePair<string, HashSet<string>> entry in orderedEntries)
                {
                    // term
                    long termLocation = fs.Position;
                    writer.Write((string)entry.Key);
                    SerializedTermsSize += fs.Position - termLocation;
                    // number of postings
                    writer.Write((Int32)entry.Value.Count);
                    // postings
                    foreach (string docId in entry.Value)
                        writer.Write((string)docId);
                }
            }

            this.postingLists.Clear();
            return filename;
        }
    }
}
