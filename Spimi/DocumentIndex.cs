using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class DocumentIndex
    {
        public DocumentIndex()
        {
            FilePaths = new Dictionary<string, string>();
        }

        public Dictionary<string, string> FilePaths { get; set; }
    }
}
