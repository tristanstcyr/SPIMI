using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class DocumentInfo
    {
        public string Url { get; private set; }
        public int Length { get; private set; }
        public string Identifier { get; private set; }

        public DocumentInfo(string url, int length, string identifier)
        {
            this.Url = url;
            this.Length = length;
            this.Identifier = identifier;
        }

        public override bool Equals(object obj)
        {
            DocumentInfo other = obj as DocumentInfo;
            return obj != null &&
                this.Url.Equals(other.Url)
                && this.Length.Equals(other.Length)
                && this.Identifier.Equals(other.Identifier);
        }

        public override string ToString()
        {
            return "DocumentInfo{" +
                "Url=" + this.Url + ", " +
                "Length=" + this.Length + ", " +
                "Identifier=" + this.Identifier + "}";
        }
    }
}
