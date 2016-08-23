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

namespace EDFPlugin.Eurostreaming
{
    public class EurostreamingPlugin : IEDFPlugin, IEDFSearch, IEDFList, IEDFContentProvider
    {
        private ESParser parser;

        public EurostreamingPlugin() { parser = new ESParser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.Eurostreaming.Serie"; }
        }

        public string pluginName
        {
            get { return "Eurostreaming"; }
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
            if(list.Equals(AvailableLists.ElementAt(0)))
                return parser.GetShowList(ESParser.Section.Film);
            else if (list.Equals(AvailableLists.ElementAt(1)))
                return parser.GetShowList(ESParser.Section.Series);
            else
                return parser.GetShowList(ESParser.Section.Anime);
        }

        public IEnumerable<string> AvailableLists
        {
            get
            {
                return new string[] { "Lista Film", "Lista Serie Tv", "Lista Cartoni Animati" };
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
