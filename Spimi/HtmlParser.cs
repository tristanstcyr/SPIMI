using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Concordia.Spimi
{
    class HtmlParser : IParser
    {

        public IEnumerable<Document> ExtractDocuments(Stream file)
        {
            StringBuilder content = new StringBuilder();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(file);

            bool isFirst = true;
            foreach (HtmlNode node in htmlDoc.DocumentNode.ChildNodes)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    content.Append(" ");
                }

                content.Append(node.InnerText);
            }

            HtmlNodeCollection titleNodes = htmlDoc.DocumentNode.SelectNodes("html/head/title");
            string title = titleNodes == null ? "" : titleNodes.ElementAt(0).InnerText;

            string text = Regex.Replace(content.ToString(), @"(\s+)", @" ").Trim();

            return new Document[] { new Document("", title, text) };

        }
    }
}
