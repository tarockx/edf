using libEraDeiFessi;
using libEraDeiFessi.Plugins;

namespace EDFPlugin.WarezBB
{
    public class WBBPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider, IEDFPluginOptions
    {
        private WBBParser parser;

        public WBBPlugin()
        {
            Options = new Options();
            parser = new WBBParser(this);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.WarezBB"; }
        }

        public string pluginName
        {
            get { return "Warez-BB"; }
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

        public object Options { get; set; }

    }

}
