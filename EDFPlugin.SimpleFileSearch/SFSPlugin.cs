using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.SimpleFileSearch
{
    public class SFSPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private SFSParser parser;

        public SFSPlugin()
        {
            parser = new SFSParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.SimpleFileSearch"; }
        }

        public string pluginName
        {
            get { return "SimpleFileSearch"; }
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
