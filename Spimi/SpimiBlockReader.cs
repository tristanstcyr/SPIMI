using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concordia.Spimi
{
    class SpimiBlockReader
    {
        /// <remarks> Keeps file open until all postings have been read</remarks>
        public IEnumerable<PostingList> Read(string filepath)
        {
            using (FileStream stream = File.Open(filepath, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(stream);
                int termCount = reader.ReadInt32();
                for (int termIndex = 0; termIndex < termCount; termIndex++)
                {
                    string term = reader.ReadString();
                    int postingCount = reader.ReadInt32();
                    List<string> postings = new List<string>(postingCount);
                    for (int postingIndex = 0; postingIndex < postingCount; postingIndex++)
                    {
                        postings.Add(reader.ReadString());
                    }

                    yield return new PostingList(term, postings);
                }
            }
        }

        public IEnumerable<PostingList> BeginBlockMerge(List<IEnumerator<PostingList>> postingListEnums)
        {
            foreach(IEnumerator<PostingList> postingLists in postingListEnums)
            {
                postingLists.MoveNext();
            }

            while(postingListEnums.Count != 0)
            {
                // There can be multiple minimum posting lists, if a term is split into multiple posting lists
                List<IEnumerator<PostingList>> minimums = new List<IEnumerator<PostingList>>();

                foreach (IEnumerator<PostingList> postingLists in postingListEnums)
                {
                    if (minimums.Count == 0)
                    {
                        minimums.Add(postingLists);
                    }
                    else
                    {
                        string term = minimums[0].Current.Term;
                        int compareResult = term.CompareTo(postingLists.Current.Term);

                        if (compareResult == 0)
                        {
                            minimums.Add(postingLists);
                        }
                        else if (compareResult > 0)
                        {
                            minimums.Clear();
                            minimums.Add(postingLists);
                        }
                    }
                }

                // Return the next posting list
                if (minimums.Count > 1)
                {
                    // Merge posting listss
                    string minimumTerm = minimums[0].Current.Term;
                    HashSet<string> mergedPostingList = new HashSet<string>();
                    foreach (IEnumerator<PostingList> postingListEnum in minimums)
                    {
                        PostingList postingList = postingListEnum.Current;
                        foreach (string posting in postingList.Postings)
                            mergedPostingList.Add(posting);
                    }
                    yield return new PostingList(minimumTerm, mergedPostingList.ToList());
                }
                else
                {
                    yield return minimums[0].Current;
                }

                // Advance blocks and remove posting empty ones
                foreach (IEnumerator<PostingList> enumerator in minimums)
                {
                    if (!enumerator.MoveNext())
                    {
                        postingListEnums.Remove(enumerator);
                    }
                }
            }
        }

        public List<IEnumerator<PostingList>> OpenBlocks(IEnumerable<string> blockFilePaths)
        {
            List<IEnumerator<PostingList>> postingLists = new List<IEnumerator<PostingList>>();
            foreach (string filePath in blockFilePaths)
            {
                postingLists.Add(Read(filePath).GetEnumerator());
            }
            return postingLists;
        }
    }
}
