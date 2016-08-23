using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.DownloadNow
{
    public class DownloadNowPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private DownloadNowParser parser;

        public DownloadNowPlugin()
        {
            parser = new DownloadNowParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.DownloadNow"; }
        }

        public string pluginName
        {
            get { return "DownloadNow"; }
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
