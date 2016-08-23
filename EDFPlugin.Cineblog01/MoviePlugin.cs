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
    public class CBMoviePlugin : IEDFPlugin, IEDFSearch, IEDFList, IEDFContentProvider
    {
        private Cineblog01Parser parser;

        public CBMoviePlugin() { parser = new Cineblog01Parser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.Cineblog01.Film"; }
        }

        public string pluginName
        {
            get { return "Cineblog01 - Film"; }
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
            return parser.PerformSearch(searchterm, Cineblog01Parser.SearchSection.Movies);
        }

        public SearchResult GetList(string list, string nextPage = "")
        {
            if(list.Equals("Lista completa"))
                return parser.GetMovieList();

            if (list.Equals("Ultime novità"))
                return parser.GetLatestMovies(nextPage);

            return null;
        }

        public IEnumerable<string> AvailableLists
        {
            get
            {
                return new string[] { "Lista completa", "Ultime novità" };
            }
        }

        public ParseResult ParsePage(Bookmark bm)
        {
            return parser.ParsePage(bm.Url);
        }

    }
}
