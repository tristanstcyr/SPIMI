using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Concordia.Spimi
{

    class FileIndex : IIndex
    {
        public const int PointerByteSize = sizeof(Int64);
        public const int TermSizeByteSize = sizeof(Int32);
        public const int TermEntryByteSize = PointerByteSize + TermSizeByteSize;

        long postingListStartPtr;
        Stream stream;
        BinaryReader reader;

        public long TermCount { get; private set; }

        FileIndex(Stream stream)
        {
            this.stream = stream;
            this.reader = new BinaryReader(stream);
        }

        /// <summary>
        /// Searches for the term using binary search
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public PostingList GetPostingList(string term)
        {
            long minEntryIndex = 0;
            long maxEntryIndex = this.TermCount - 1;

            while (minEntryIndex <= maxEntryIndex)
            {
                long midEntryIndex = minEntryIndex + ((maxEntryIndex - minEntryIndex) / 2);

                // Read the entry
                Int64 termEntryPtr = sizeof(long) + midEntryIndex * TermEntryByteSize;
                stream.Seek(termEntryPtr, SeekOrigin.Begin);
                Int64 termPtr = reader.ReadInt64() + sizeof(Int64) + this.TermCount * TermEntryByteSize;
                Int32 frequency = reader.ReadInt32();

                // Read the term
                stream.Seek(termPtr, SeekOrigin.Begin);
                string foundTerm = reader.ReadString();

                int comparison = term.CompareTo(foundTerm);

                if (comparison == 0)
                {
                    // Read the postings
                    List<string> postings = new List<string>();
                    for (int postingIndex = 0; postingIndex < frequency; postingIndex++)
                    {
                        postings.Add(reader.ReadString());
                    }
                    return new PostingList(term, postings);
                }
                else if (comparison > 0)
                {
                    minEntryIndex = midEntryIndex + 1;
                }
                else // if (comparison < 0)
                {
                    maxEntryIndex = midEntryIndex - 1;
                }
            }

            return new PostingList(term, new List<string>());
        }

        void initialize()
        {
            this.stream.Seek(0, SeekOrigin.Begin);
            this.TermCount = this.reader.ReadInt64();
            this.postingListStartPtr = TermEntryByteSize * this.TermCount + PointerByteSize;
        }

        public static FileIndex Open(Stream stream)
        {
            FileIndex index = new FileIndex(stream);
            index.initialize();
            return index;
        }
    }
}
