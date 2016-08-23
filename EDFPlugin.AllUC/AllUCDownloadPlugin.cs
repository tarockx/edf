using libEraDeiFessi;
using libEraDeiFessi.Plugins;

namespace EDFPlugin.AllUC
{
    public class AllUCDownloadPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private AllUCParser parser;

        public AllUCDownloadPlugin()
        {
            //WatiN.Core.Settings.Instance.MakeNewIeInstanceVisible = false;
            //WatiN.Core.Settings.Instance.AutoMoveMousePointerToTopLeft = false;
            parser = new AllUCParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.AllUCDownload"; }
        }

        public string pluginName
        {
            get { return "AllUC - DownloadSearch"; }
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
            return parser.PerformSearch(searchterm, false);
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
