using libEraDeiFessi;
using libEraDeiFessi.Plugins;

namespace EDFPlugin.RARBG
{
    public class RARBGPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private RARBGParser parser;

        public RARBGPlugin()
        {
            parser = new RARBGParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.RARBG"; }
        }

        public string pluginName
        {
            get { return "RARBG"; }
        }

        public string pluginAuthor
        {
            get { return "Master_T"; }
        }

        public string pluginVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public SearchResult PerformSearch(string searchterm)
        {
            return parser.PerformSearch(searchterm);
        }

        public ParseResult ParsePage(Bookmark bm)
        {
            return parser.ParsePage(bm.Url);
        }

        public SearchResult GetResultPage(string uri)
        {
            return parser.GetResultPage(uri);
        }

    }

}
