using libEraDeiFessi.Plugins;
using libEraDeiFessi;

namespace EDFPlugin.FilmPerTutti
{
    public class FPTPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private Parser parser;

        public FPTPlugin() { parser = new Parser(pluginID); }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.FilmPerTutti"; }
        }

        public string pluginName
        {
            get { return "FilmPerTutti"; }
        }

        public string pluginAuthor
        {
            get { return "Master_T"; }
        }

        public string pluginVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
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
