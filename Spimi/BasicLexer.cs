using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Concordia.Spimi
{
    class BasicLexer : ILexer
    {
        HashSet<char> ignoreList = new HashSet<char>();

        public BasicLexer()
        {
            char[] ignore = {' ', '\t', '\n', '.', ',', '\r'};
            foreach (char c in ignore)
            {
                ignoreList.Add(c);
            }
        }

        public IEnumerable<string> tokenize(string document)
        {   
            StringBuilder token = new StringBuilder();
            foreach(char character in document.ToCharArray()) 
            {
                if (ignoreList.Contains(character))
                {
                    if (token.Length > 0)
                    {
                        string result = token.ToString().ToLower();
                        token.Clear();
                        yield return result;
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
