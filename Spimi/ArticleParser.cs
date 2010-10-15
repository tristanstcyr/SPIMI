using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Concordia.Spimi
{
    class ArticleParser : IParser
    {
        public IEnumerable<Document> scrub(Stream file)
        {
            StreamReader reader = new StreamReader(file);
            Document doc = null;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.Contains("<REUTERS TOPICS"))    // start of a new document
                {
                    doc = new Document();
                    doc.DocId = GetDocId(line);
                }
                else if (line.Contains("</REUTERS>"))    // end of a document
                {
                    yield return doc;
                }
                else if (line.Contains("<!DOCTYPE") ||       // lines that can be skipped because they have no content
                            line.Contains("</TOPICS>") || line.Contains("<TOPICS>") ||
                            line.Contains("<PLACES>") || line.Contains("<PLACES>") ||
                            line.Contains("<PEOPLE>") || line.Contains("</PEOPLE>") ||
                            line.Contains("<ORGS>") || line.Contains("</ORGS>") ||
                            line.Contains("<EXCHANGES>") || line.Contains("</EXCHANGES>") ||
                            line.Contains("<COMPANIES>") || line.Contains("</COMPANIES>") ||
                            line.Contains("<UNKNOWN>") || line.Contains("</UNKNOWN>") ||
                            line.Contains("<TEXT>") || line.Contains("</TEXT>") ||
                            line.Contains("</BODY>"))
                {
                    continue;
                }
                else if (doc != null)   // regular line to include in the document
                {
                    string cleanLine = ScrubOutMarkup(line);
                    doc.Body = doc.Body + cleanLine;
                }
            }
        }

        private string GetDocId(string line)
        {
            int idLabelPos = line.IndexOf("NEWID");
            int idPos = idLabelPos + 6;
            string endOfLine = line.Substring(idPos);
            return endOfLine.Replace("\"", "").Replace(">", "");
        }

        private string ScrubOutMarkup(string line)
        {
            return line.Replace("<DATE>", "").Replace("</DATE>", "")
                .Replace("<D>", "").Replace("</D>", "")
                .Replace("<TITLE>", "").Replace("</TITLE>", "")
                .Replace("<DATELINE>", "").Replace("</DATELINE><BODY>", "");
        }

    }
}
