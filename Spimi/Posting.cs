using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    [Serializable]
    class Posting
    {
        public long DocumentId { get; set; }
        public int Frequency { get; set; }

        public Posting(long documentId, int frequency)
        {
            this.DocumentId = documentId;
            this.Frequency = frequency;
        }

        public override bool Equals(object obj)
        {
            Posting other = obj as Posting;
            return other != null
                && other.DocumentId.Equals(this.DocumentId)
                && other.Frequency.Equals(this.Frequency);
        }

        public override string ToString()
        {
            return "Posting{DocumentId="+this.DocumentId+", Frequency="+this.Frequency+">";
        }
    }
}
