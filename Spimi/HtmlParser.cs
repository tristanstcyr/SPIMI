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
        static string[] TagsToParse =
        {
            "p", "h1","h2","h3","h4","h5","h6","div", "title"
        };

        public IEnumerable<Document> ExtractDocuments(Stream file)
        {
            StringBuilder content = new StringBuilder();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(file);

            bool isFirst = true;
            // TODO: There must be a better way of doing this
            foreach (string tag in TagsToParse)
            {
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//" + tag);
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            content.Append(" ");
                        }

                        content.Append(node.InnerText.Replace("\n", " "));
                    }
                }
            }

            HtmlNodeCollection titleNodes = htmlDoc.DocumentNode.SelectNodes("html/head/title");
            string title = titleNodes == null ? "" : titleNodes.ElementAt(0).InnerText.Replace("\n", " ");

            string text = Regex.Replace(content.ToString(), @"(\s+)", @" ").Trim();

            return new Document[] { new Document("", title, text) };

        }
    }
}
