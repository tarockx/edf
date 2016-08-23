using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.Filespr
{
    public class FsprPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private FsprParser parser;

        public FsprPlugin()
        {
            parser = new FsprParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.Filespr"; }
        }

        public string pluginName
        {
            get { return "FileSpr"; }
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
