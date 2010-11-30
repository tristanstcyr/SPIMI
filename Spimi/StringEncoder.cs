using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class StringEncoder : IBinaryObjectEncoder<string>
    {
        public void write(System.IO.BinaryWriter stream, string t)
        {
            stream.Write(t);
        }

        public string read(System.IO.BinaryReader stream)
        {
            return stream.ReadString();
        }
    }
}
