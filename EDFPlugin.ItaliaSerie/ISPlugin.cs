using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libEraDeiFessi.Plugins;
using System.Net;
using System.IO;
using libEraDeiFessi;
using HtmlAgilityPack;

namespace EDFPlugin.ItaliaSerie
{
    public class ItaliSeriePlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private ISParser parser;

        public ItaliSeriePlugin() { parser = new ISParser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.ItaliaSerie"; }
        }

        public string pluginName
        {
            get { return "ItaliSerie"; }
        }

        public string pluginAuthor
        {
            get { return "Master_T"; }
        }

        public string pluginVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public IEnumerable<libEraDeiFessi.Bookmark> GetBookmarks()
        {
            throw new NotImplementedException();
        }

        public ParseResult ParsePage(Bookmark bm)
        {
            return parser.ParsePage(bm.Url);
        }

        public SearchResult PerformSearch(string searchterm)
        {
            return parser.PerformSearch(searchterm);
        }

        public SearchResult GetResultPage(string uri)
        {
            return parser.GetResultPage(uri);
        }




    }
}
