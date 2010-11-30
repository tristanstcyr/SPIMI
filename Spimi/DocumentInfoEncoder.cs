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
            stream.Write((string)t.Url);
            stream.Write((string)t.Identifier);
        }

        public DocumentInfo read(System.IO.BinaryReader stream)
        {
            int length = stream.ReadInt32();
            string url = stream.ReadString();
            string id = stream.ReadString();
            return new DocumentInfo(url, length, id);
        }
    }
}
