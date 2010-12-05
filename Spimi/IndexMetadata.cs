using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Concordia.Spimi
{
    public class IndexMetadata
    {
        FileIndex<long, DocumentInfo> documentsInfo;

        public long TokenCount { get; private set; }

        public IndexMetadata(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            this.TokenCount = reader.ReadInt64();
            this.documentsInfo = new FileIndex<long, DocumentInfo>(
                new LongEncoder(),
                new DocumentInfoEncoder(), stream);
        }

        public bool TryGetDocumentInfo(long documentId, out DocumentInfo docInf)
        {
            return documentsInfo.TryGet(documentId, out docInf);
        }

        public IEnumerable<DocumentInfo> GetDocuments(IEnumerable<long> documentIds)
        {
            foreach (long docId in documentIds)
            {
                yield return this[docId];
            }
        }

        public DocumentInfo this[long documentId]
        {
            get
            {
                DocumentInfo info;
                if (!this.TryGetDocumentInfo(documentId, out info))
                    throw new ArgumentOutOfRangeException("documentId "+documentId+" is not in the metadata");
                return info;
            }

        }

        public long CollectionLengthInDocuments
        {
            get { return documentsInfo.EntryCount; }
        }

        public IList<long> GetDocumentIds()
        {
            return new DocumentInfoList(this.documentsInfo);
        }

        class DocumentInfoList : IList<long>
        {
            FileIndex<long, DocumentInfo> documentsInfo;

            public DocumentInfoList(FileIndex<long, DocumentInfo> documentsInfo)
            {
                this.documentsInfo = documentsInfo;
            }


            public int IndexOf(long item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, long item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public long this[int index]
            {
                get
                {
                    return documentsInfo.GetKey(index);
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public void Add(long item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(long item)
            {
                Contract.Requires<IndexOutOfRangeException>(item >= 0);
                return item < this.documentsInfo.EntryCount;
                
            }

            public void CopyTo(long[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { return (int)this.documentsInfo.EntryCount;  }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(long item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<long> GetEnumerator()
            {
                long count = this.documentsInfo.EntryCount;
                for (int i = 0; i < count; i++)
                {
                    yield return this.documentsInfo.GetKey(i);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
