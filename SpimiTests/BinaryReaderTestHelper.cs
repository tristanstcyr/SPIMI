using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Concordia.Spimi;

namespace Concordia.SpimiTests
{
    class BinaryReaderTestHelper
    {
        public static Posting readPosting(BinaryReader reader)
        {
            return new Posting(reader.ReadString(), reader.ReadInt32());
        }
    }
}
