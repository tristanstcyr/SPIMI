using System;
using System.Collections.Generic;
namespace Concordia.Spimi
{
    interface ILexer
    {
        IEnumerable<string> tokenize(string document);
    }
}
