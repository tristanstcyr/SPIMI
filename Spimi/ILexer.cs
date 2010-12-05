using System;
using System.Collections.Generic;
namespace Concordia.Spimi
{
    public interface ILexer
    {
        IEnumerable<string> Tokenize(string document);
    }
}
