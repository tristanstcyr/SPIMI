using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    class IndexMetadata
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
    }
}
