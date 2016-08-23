using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.ShareDir
{
    public class SDPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private SDParser parser;

        public SDPlugin()
        {
            parser = new SDParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.ShareDir"; }
        }

        public string pluginName
        {
            get { return "ShareDir"; }
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
