﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Concordia.Spimi
{
    class ArticleParser : IParser
    {
        private Regex skipRegex = new Regex("<!DOCTYPE|</TOPICS>|<TOPICS>|<PLACES>|<PLACES>|<PEOPLE>|</PEOPLE>|<ORGS>|</ORGS>|<EXCHANGES>|</EXCHANGES>|<COMPANIES>|</COMPANIES>|<UNKNOWN>|</UNKNOWN>|<TEXT>|</TEXT>|</BODY>");
        private Regex markupRegex = new Regex("&#\\d*\\;|<DATE>|</DATE>|<D>|</D>|<TITLE>|</TITLE>|<DATELINE>|</DATELINE><BODY>");

        public IEnumerable<Document> ExtractDocuments(Stream file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                StringBuilder bodyBuilder = new StringBuilder();
                string docId = "";
                bool inADoc = false;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Contains("<REUTERS TOPICS"))    // start of a new document
                    {
                        docId = GetDocId(line);
                        inADoc = true;
                    }
                    else if (line.Contains("</REUTERS>"))    // end of a document
                    {
                        inADoc = false;
                        string body = bodyBuilder.ToString().ToLower();
                        bodyBuilder.Clear();
                        yield return new Document(docId, body);
                    }
                    else if (skipRegex.IsMatch(line))   // lines that can be skipped because they have no content
                    {
                        continue;
                    }
                    else if (inADoc)   // regular line to include in the document
                    {
                        string cleanLine = ScrubOutMarkup(line);
                        bodyBuilder.Append(cleanLine);
                    }
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
            Match match = markupRegex.Match(line);
            while (match.Success)
            {
                line = line.Remove(match.Index, match.Length);
                match = markupRegex.Match(line);
            }
            return line;
        }

    }
}
