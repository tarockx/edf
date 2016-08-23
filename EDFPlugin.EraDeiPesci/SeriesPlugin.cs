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

namespace EDFPlugin.EraDeiPesci
{
    public class SeriesPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private EDPParser parser;

        public SeriesPlugin() { parser = new EDPParser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.EraDeiPesci.Serie"; }
        }

        public string pluginName
        {
            get { return "Era Dei Pesci"; }
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

        public SearchResult GetList(string list, string nextPage = "")
        {
            return parser.GetShowList();
        }

        public IEnumerable<string> AvailableLists
        {
            get
            {
                return new string[] { "Lista completa" };
            }
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
