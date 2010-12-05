using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    public interface IBinaryObjectEncoder<T>
    {
        void write(BinaryWriter stream, T t);
        T read(BinaryReader stream);
    }
}
