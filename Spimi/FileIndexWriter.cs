using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Concordia.Spimi
{
    /// <summary>
    /// Creates the FileIndex.
    /// </summary>
    class FileIndexWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termEntriesStream"></param>
        /// <param name="postingLists"></param>
        public void Write(Stream termEntriesStream, IEnumerable<PostingList> postingLists)
        {
            // +------------------------------------------------------------------------------------------------------------+
            // | 1. (8) Term count | 2. (8) Term ptr, (4) Doc Frequency | .... | 3. Term, Posting | ... |
            // +------------------------------------------------------------------------------------------------------------+
            //                                                      ^                                        ^   
            //                                             termEntryPtr                               postingListPtr

            BinaryWriter termEntriesWriter = new BinaryWriter(termEntriesStream);
            using (FileStream postingListsStream = File.Open(Path.GetTempFileName(), FileMode.Open))
            {
                BinaryWriter postingListsWriter = new BinaryWriter(postingListsStream);

                // 1. (8) Term count
                termEntriesWriter.Write((Int64)0);

                long termEntryPtr = termEntriesStream.Position;
                long postingListPtr = 0;
                long entryCount = 0;
                foreach (PostingList postingList in postingLists)
                {
                    long termPtr = postingListPtr;

                    // 3.
                    postingListsStream.Seek(postingListPtr, SeekOrigin.Begin);
                    // Term
                    postingListsWriter.Write((string)postingList.Term);
                    // Posting list
                    foreach (Posting posting in postingList.Postings)
                    {
                        postingListsWriter.Write((string)posting.DocumentId);
                        postingListsWriter.Write((Int32)posting.Frequency);
                    }

                    postingListPtr = postingListsStream.Position;

                    // 2. 
                    termEntriesStream.Seek(termEntryPtr, SeekOrigin.Begin);
                    // (8) Term ptr
                    termEntriesWriter.Write((Int64)termPtr);
                    // (4) frequency
                    termEntriesWriter.Write((Int32)postingList.Postings.Count);

                    termEntryPtr = termEntriesStream.Position;
                
                    entryCount++;
                }
                termEntriesStream.Seek(0, SeekOrigin.Begin);
                termEntriesWriter.Write((Int64)entryCount);

                // Copy the postingLists buffer to the termEntries
                termEntriesStream.Seek(termEntryPtr, SeekOrigin.Begin);
                postingListsStream.Seek(0, SeekOrigin.Begin);
                postingListsStream.CopyTo(termEntriesStream);
            }
        }
    }
}
