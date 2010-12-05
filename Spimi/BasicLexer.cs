using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Concordia.Spimi
{
    /// <summary>
    /// Lexer that can read and tokenize a document
    /// </summary>
    class BasicLexer : ILexer
    {
        HashSet<char> ignoreList = new HashSet<char>();
        private List<string> stopList;



        public BasicLexer()
        {
            char[] ignore = {' ', '\t', '\n', '.', ',', '\r'};
            foreach (char c in ignore)
            {
                ignoreList.Add(c);
            }

            // Remove html encodings and most common semantically nonselective term of Reuters-RCV1 (see Manning IR textbook)
            this.stopList = new List<string>() {"&quot;", "&lt;", "&gt;", "&amp;", "a", "an", "and", "are", "at", "to", "be", "by", "for", "from", "has", "he", "in",
                                                "is", "it", "its", "of", "on", "that", "the", "to", "was", "were", "will", "with"};
        }

        /// <summary>
        /// Iterator that builds tokens from the inputted document, and returns them as they are built.
        /// </summary>
        /// <param name="document">The document to tokenize</param>
        /// <returns>Returns lowercase tokens, one at a time as soon as they are read</returns>
        public IEnumerable<string> Tokenize(string document)
        {   
            StringBuilder token = new StringBuilder();
            foreach(char character in document) 
            {
                if (ignoreList.Contains(character))
                {
                    if (token.Length > 0)
                    {
                        string result = token.ToString().ToLower();
                        token.Clear();
                        if (!this.stopList.Contains(result))    // dont return stop words
                        {
                            yield return result;
                        }
                        
                    }
                }
                else
                {
                    token.Append(character);
                }
            }

            if (token.Length > 0)
            {
                yield return token.ToString().ToLower();
            }
        }
    }
}
