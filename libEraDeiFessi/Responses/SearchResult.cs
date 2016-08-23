using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class SearchResult
    {
        public ICollection<Bookmark> Result { get; set; }
        public string NextPageUrl { get; set; }
        public string Error { get; set; }
        public bool HasError { get { return !string.IsNullOrEmpty(Error); } }

    }
}
