using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class Link
    {
        public string Text { get; set; }
        public string Url { get; set; }

        public Link(string text, string url){
            Text = text;
            Url = url;
        }
    }
}
