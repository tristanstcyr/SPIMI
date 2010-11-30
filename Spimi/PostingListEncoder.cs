using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    class PostingListEncoder : IBinaryObjectEncoder<IList<Posting>>
    {

        public void write(BinaryWriter writer, IList<Posting> postingList)
        {
            writer.Write((Int32)postingList.Count);
            foreach(Posting posting in postingList)
            {
                writer.Write((Int32)posting.Frequency);
                writer.Write((Int64)posting.DocumentId);
            }
        }

        public IList<Posting> read(BinaryReader reader)
        {
            IList<Posting> postings = new List<Posting>();
            Int32 postingCount = reader.ReadInt32();
            for (int postingIndex = 0; postingIndex < postingCount; postingIndex++)
            {
                Int32 frequency = reader.ReadInt32();
                Int64 docId = reader.ReadInt64();
                postings.Add(new Posting(docId, frequency));
            }

            return postings;
        }
    }
}
