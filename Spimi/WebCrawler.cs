using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.Contracts;

namespace Concordia.Spimi
{
    class WebCrawler
    {
        DirectoryInfo directory;

        [ContractInvariantMethod]
        public void invariants() 
        {
            Contract.Invariant(directory != null);
        }

        public WebCrawler(DirectoryInfo directory) {
            Contract.Requires(directory.Exists);
            
            this.directory = directory;
        }

        public IEnumerable<WebDocument> GetDocuments()
        {
            return GetDocumentsRecursiveDepthFirst("http://", this.directory);
        }

        private static IEnumerable<WebDocument> GetDocumentsRecursiveDepthFirst(string uri, DirectoryInfo directory)
        {
            string uriStart = uri + directory.Name + "/";

            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                foreach (WebDocument doc in GetDocumentsRecursiveDepthFirst(uriStart, subDirectory))
                {
                    yield return doc;
                }
            }

            foreach(FileInfo file in directory.GetFiles())
            {
                string fileUri = uriStart + (file.Name.Contains("index.") ? "" : file.Name);
                yield return new WebDocument(fileUri, file);
            }
        }
    }
}
