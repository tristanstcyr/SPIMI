using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    public class WebDocument
    {
        public string Uri { get; private set; }

        private FileInfo file;

        public WebDocument(string uri, FileInfo file)
        {
            this.Uri = uri;
            this.file = file;
        }

        public Stream Open()
        {
            return this.file.Open(FileMode.Open);
        }
    }
}
