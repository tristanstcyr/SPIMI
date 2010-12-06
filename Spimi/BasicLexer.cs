using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Concordia.Spimi
{
    /// <summary>
    /// Lexer that can read and tokenize a document
    /// </summary>
    public class BasicLexer : ILexer
    {
        HashSet<char> ignoreList = new HashSet<char>();
        private string[] stopList;



        public BasicLexer()
        {
            char[] ignore = "\t\n.,\r-;:|()[]?! <>\"".ToCharArray();
            foreach (char c in ignore)
                ignoreList.Add(c);

            this.stopList =
                ("a,able,about,across,after,all,almost,also,am,among,an,and," +
                "any,are,as,at,be,because,been,but,by,can,cannot,could,dear," +
                "did,do,does,either,else,ever,every,for,from,get,got,had,has," +
                "have,he,her,hers,him,his,how,however,i,if,in,into,is,it,its," +
                "just,least,let,like,likely,may,me,might,most,must,my,neither," +
                "no,nor,not,of,off,often,on,only,or,other,our,own,rather,said," +
                "say,says,she,should,since,so,some,than,that,the,their,them,then," +
                "there,these,they,this,tis,to,too,twas,us,wants,was,we,were,what,when," +
                "where,which,while,who,whom,why,will,with,would,yet,you,your").Split(',');
        }


        private bool isValidTerm(string term)
        {
            return !this.stopList.Contains(term) &&
                !term.Contains("&") &&
                term.Length > 1;
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
                        if (this.isValidTerm(result))    // dont return stop words
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
