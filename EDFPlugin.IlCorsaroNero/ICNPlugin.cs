using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.IlCorsaroNero
{
    public class ICNPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private ICNParser parser;

        public ICNPlugin()
        {
            parser = new ICNParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.IlCorsaroNero"; }
        }

        public string pluginName
        {
            get { return "IlCorsaroNero"; }
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
