using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class TorrentContent
    {
        public string MagnetURI { get; set; }
        public string Description { get; set; }
        public string HtmlDescription { get; set; }
        public string CoverImageUrl { get; set; }
        public string Title { get; set; }

        public TorrentContent()
        {
        }
    }
}
