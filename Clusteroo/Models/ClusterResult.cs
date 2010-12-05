using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clusteroo.Models
{
    public class ClusterResult
    {
        private IList<string> topTerms;

        public IList<string> TopTerms
        {
            get { return topTerms; }
            set { topTerms = value; }
        }
        private IList<string> docUris;

        public IList<string> DocUris
        {
            get { return docUris; }
            set { docUris = value; }
        }

        public ClusterResult(IList<string> topTerms, IList<string> docUris)
        {
            this.topTerms = topTerms;
            this.docUris = docUris;
        }

    }
}
