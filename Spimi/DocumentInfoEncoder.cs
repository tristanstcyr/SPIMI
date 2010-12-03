using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class DocumentInfoEncoder : IBinaryObjectEncoder<DocumentInfo>
    {

        public void write(System.IO.BinaryWriter stream, DocumentInfo t)
        {
            stream.Write((Int32)t.Length);
            stream.Write((string)t.Uri);
            stream.Write((string)t.Title);
            stream.Write((string)t.SpecialIdentifier);
        }

        public DocumentInfo read(System.IO.BinaryReader stream)
        {
            int length = stream.ReadInt32();
            string url = stream.ReadString();
            string title = stream.ReadString();
            string id = stream.ReadString();
            return new DocumentInfo(url, title, length, id);
        }
    }
}
