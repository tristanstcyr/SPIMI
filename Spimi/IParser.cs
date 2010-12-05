﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    public interface IParser
    {
        IEnumerable<Document> ExtractDocuments(Stream file);
    }
}
