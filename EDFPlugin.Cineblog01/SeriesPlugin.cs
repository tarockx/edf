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

namespace EDFPlugin.Cineblog01
{
    public class CBSeriesPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private Cineblog01Parser parser;

        public CBSeriesPlugin() { parser = new Cineblog01Parser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.Cineblog01.Series"; }
        }

        public string pluginName
        {
            get { return "Cineblog01 - Serie TV"; }
        }

        public string pluginAuthor
        {
            get { return "Master_T"; }
        }

        public string pluginVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public SearchResult GetResultPage(string uri)
        {
            return parser.GetResultPage(uri);
        }

        public SearchResult PerformSearch(string searchterm)
        {
            return parser.PerformSearch(searchterm, Cineblog01Parser.SearchSection.Series);
        }

        public ParseResult ParsePage(Bookmark bm)
        {
            return parser.ParsePage(bm.Url);
        }

    }
}
