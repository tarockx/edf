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

namespace EDFPlugin.PirateStreaming
{
    public class PirateStreamingSeriesPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider, IEDFList
    {
        private PSParser parser;

        public PirateStreamingSeriesPlugin() { parser = new PSParser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.PirateStreaming.Serie"; }
        }

        public string pluginName
        {
            get { return "PirateStreaming - Serie TV"; }
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

        public IEnumerable<libEraDeiFessi.Bookmark> GetList()
        {
            return parser.GetShowList();
        }

        public ParseResult ParsePage(Bookmark bm)
        {
            return parser.ParseShowPage(bm.Url);
        }

        public SearchResult PerformSearch(string searchterm)
        {
            return parser.PerformSearch(searchterm, PSParser.ContentType.Shows);
        }

        public SearchResult GetResultPage(string uri)
        {
            return parser.GetResultPage(uri, PSParser.ContentType.Shows);
        }

        public SearchResult GetList(string list, string searchpage = "")
        {
            var res = parser.GetResultPage(string.IsNullOrEmpty(searchpage) ? Constants.Liste[list] : searchpage, PSParser.ContentType.Shows);
            if (res == null)
                return res;

            while (!list.Equals("Ultime novità") && !string.IsNullOrEmpty(res.NextPageUrl))
            {
                var newPage = parser.GetResultPage(res.NextPageUrl, PSParser.ContentType.Shows);
                if (newPage != null && newPage.Result != null)
                {
                    (res.Result as List<Bookmark>).AddRange(newPage.Result);
                    res.NextPageUrl = newPage.NextPageUrl;
                }
                else
                {
                    res.NextPageUrl = string.Empty;
                }

            }
            return res;
            //return (parser.GetResultPage(string.IsNullOrEmpty(searchpage) ? Constants.Liste[list] : searchpage, PSParser.ContentType.Shows));
        }


        public IEnumerable<string> AvailableLists
        {
            get { return Constants.Liste.Keys; }
        }
    }
}
