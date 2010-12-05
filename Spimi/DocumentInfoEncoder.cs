using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class DocumentInfoEncoder : IBinaryObjectEncoder<DocumentInfo>
    {
        TermVectorEncoder termVectorEncoder = new TermVectorEncoder();

        public void write(System.IO.BinaryWriter stream, DocumentInfo t)
        {
            stream.Write((Int32)t.Length);
            stream.Write((string)t.Uri);
            stream.Write((string)t.Title);
            stream.Write((string)t.SpecialIdentifier);
            termVectorEncoder.write(stream, t.TermVector);
        }

        public DocumentInfo read(System.IO.BinaryReader stream)
        {
            int length = stream.ReadInt32();
            string url = stream.ReadString();
            string title = stream.ReadString();
            string id = stream.ReadString();
            TermVector vector = termVectorEncoder.read(stream);
            return new DocumentInfo(url, title, length, id, vector);
        }
    }
}
