using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class IndexMetadata
    {
        private Dictionary<string, string> documentMap = new Dictionary<string, string>();
        private Dictionary<string, int> documentLengthMap = new Dictionary<string, int>();
        private long collectionLengthInTokens;
        private long collectionLengthInDocuments;

        public IndexMetadata(Dictionary<string, string> documentMap, Dictionary<string, int> documentLengthMap, long collectionLengthInDocuments, long collectionLengthInTokens)
        {
            // TODO: Complete member initialization
            this.documentMap = documentMap;
            this.documentLengthMap = documentLengthMap;
            this.collectionLengthInDocuments = collectionLengthInDocuments;
            this.collectionLengthInTokens = collectionLengthInTokens;
        }

        public Dictionary<string, int> DocumentLengthMap
        {
            get { return documentLengthMap; }
            set { documentLengthMap = value; }
        }

        public long CollectionLengthInTokens
        {
            get { return collectionLengthInTokens; }
        }

        public long CollectionLengthInDocuments
        {
            get { return collectionLengthInDocuments; }
        }


        /// <summary>
        /// Helper method that returns the file in which the specified document can be found.
        /// </summary>
        public string FilePathForDocId(string docId)
        {
            int docIdInt = int.Parse(docId);
            int lastDocId = 1;
            foreach (string key in documentMap.Keys.OrderBy(k => int.Parse(k)))
            {
                int intKey = int.Parse(key);
                if (intKey > docIdInt)
                    return documentMap[lastDocId.ToString()];
                else if (intKey == docIdInt)
                    return documentMap[key];
                lastDocId = intKey;
            }
            return documentMap[lastDocId.ToString()];
        }

    }
}
