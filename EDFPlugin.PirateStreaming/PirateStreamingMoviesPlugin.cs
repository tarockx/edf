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
    public class PirateStreamingMoviesPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private PSParser parser;

        public PirateStreamingMoviesPlugin() { parser = new PSParser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.PirateStreaming.Film"; }
        }

        public string pluginName
        {
            get { return "PirateStreaming - Film"; }
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
            return parser.ParseMoviePage(bm.Url);
        }

        public SearchResult PerformSearch(string searchterm)
        {
            return parser.PerformSearch(searchterm, PSParser.ContentType.Movies);
        }

        public SearchResult GetResultPage(string uri)
        {
            return parser.GetResultPage(uri, PSParser.ContentType.Movies);
        }

    }
}
