using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.FilesTube
{
    public class FTPlugin : IEDFPlugin, IEDFSearch, IEDFContentProvider, IEDFPluginOptions
    {
        private FTParser parser;

        public FTPlugin() { 
            parser = new FTParser(pluginID);
            Options = new Options();
        }

        public string pluginID
        {
            get { return "EraDeiFessi.Plugin.FilesTube"; }
        }

        public string pluginName
        {
            get { return "FilesTube"; }
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
            return parser.PerformSearch(searchterm, (Options as Options).Extension.ToString());
        }

        public SearchResult GetResultPage(string uri)
        {
            return parser.GetResultPage(uri);
        }

        public object Options { get; set; }

    }

}
