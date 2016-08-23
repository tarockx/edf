using libEraDeiFessi;
using libEraDeiFessi.Plugins;

namespace EDFPlugin.ThePirateBay
{
    public class TPBPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private TPBParser parser;

        public TPBPlugin()
        {
            parser = new TPBParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.ThePirateBay"; }
        }

        public string pluginName
        {
            get { return "The Pirate Bay"; }
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
