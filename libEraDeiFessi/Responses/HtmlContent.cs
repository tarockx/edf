using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class HtmlContent
    {
        public string Content { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }

        public bool HideDescriptionPanel { get; set; } = false;
    }
}
