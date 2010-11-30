using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class LongEncoder : IBinaryObjectEncoder<long>
    {
        public void write(System.IO.BinaryWriter stream, long t)
        {
            stream.Write(t);
        }

        public long read(System.IO.BinaryReader stream)
        {
            return stream.ReadInt64();
        }
    }
}
