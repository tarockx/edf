using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.AllUC
{
    public class AllUCStreamPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider
    {
        private AllUCParser parser;

        public AllUCStreamPlugin()
        {
            //WatiN.Core.Settings.Instance.MakeNewIeInstanceVisible = false;
            //WatiN.Core.Settings.Instance.AutoMoveMousePointerToTopLeft = false;
            parser = new AllUCParser(pluginID);
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.AllUCStream"; }
        }

        public string pluginName
        {
            get { return "AllUC - StreamingSearch"; }
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
            return parser.PerformSearch(searchterm, true);
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
