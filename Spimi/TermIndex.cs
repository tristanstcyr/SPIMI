using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    class TermIndex : FileIndex<string, IList<Posting>>
    {
        public TermIndex(Stream stream) : 
            base(new StringEncoder(), new PostingListEncoder(), stream) {}
    }
}
