using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class TermEntry
    {
        string Term { get; set; }
        long PostingListPtr {get;set;}
    }
}
