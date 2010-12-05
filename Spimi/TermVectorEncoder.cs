using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    class TermVectorEncoder : IBinaryObjectEncoder<TermVector>
    {
        public void write(BinaryWriter writer, TermVector vector)
        {
            ICollection<string> terms = vector.Terms;
            writer.Write((Int32)terms.Count);
            foreach (string term in terms)
            {
                int length = vector.GetDimensionLength(term);
                writer.Write((string)term);
                writer.Write((Int32)length);
            }
        }

        public TermVector read(BinaryReader reader)
        {
            Dictionary<string, int> lengths = new Dictionary<string, int>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string term = reader.ReadString();
                Int32 length = reader.ReadInt32();
                lengths.Add(term, length);
            }

            return new TermVector(lengths);
        }
    }
}
