using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    class CollectionMetadataWriter : IDisposable
    {
        const int HeaderSize = sizeof(long);

        FileIndexWriter<long, DocumentInfo> documentsInfo;

        Stream stream;

        long collectionTokenCount = 0;

        public CollectionMetadataWriter(Stream stream)
        {
            this.stream = stream;
            
            // Start the index past the head
            this.stream.Seek(HeaderSize, SeekOrigin.Begin);

            this.documentsInfo = new FileIndexWriter<long, DocumentInfo>(
                new LongEncoder(),
                new DocumentInfoEncoder(), stream);
        }

        public void WriteOut()
        {
            documentsInfo.WriteOut();

            this.stream.Seek(0, SeekOrigin.Begin);
            BinaryWriter writer = new BinaryWriter(this.stream);
            writer.Write((Int64)collectionTokenCount);
            this.stream.Seek(0, SeekOrigin.Begin);
        }

        internal void AddDocumentInfo(long documentId, DocumentInfo documentInfo)
        {
            documentsInfo.Add(documentId, documentInfo);
            collectionTokenCount += documentInfo.Length;
        }

        public void Dispose()
        {
            documentsInfo.Dispose();
        }
    }
}
