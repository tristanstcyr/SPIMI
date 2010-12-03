using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class DocumentInfo
    {
        public string Uri { get; private set; }
        public int Length { get; private set; }
        public string SpecialIdentifier { get; private set; }
        public string Title { get; private set; }
        public TermVector TermVector { get; private set; }

        public DocumentInfo(string uri, string title, int length, string identifier, TermVector termVector)
        {
            this.Uri = uri;
            this.Length = length;
            this.SpecialIdentifier = identifier;
            this.Title = title;
            this.TermVector = termVector;
        }

        public override bool Equals(object obj)
        {
            DocumentInfo other = obj as DocumentInfo;
            return obj != null &&
                this.Uri.Equals(other.Uri)
                && this.Length.Equals(other.Length)
                && this.SpecialIdentifier.Equals(other.SpecialIdentifier);
        }

        public override int GetHashCode()
        {
            return (Uri+SpecialIdentifier).GetHashCode();
        }

        public override string ToString()
        {
            return "DocumentInfo{" +
                "Uri=" + this.Uri + ", " +
                "Length=" + this.Length + ", " +
                "Identifier=" + this.SpecialIdentifier + "}";
        }
    }
}
