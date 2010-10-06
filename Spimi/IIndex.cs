using System;
using System.Collections.Generic;
namespace Concordia.Spimi
{
    interface IIndex
    {
        PostingList GetPostingList(string term);
    }
}
