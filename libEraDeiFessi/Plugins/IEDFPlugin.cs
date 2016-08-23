using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi.Plugins
{
    public interface IEDFPlugin
    {
        string pluginID { get; }
        string pluginName { get; }
        string pluginAuthor { get; }
        string pluginVersion { get; }

    }

    public interface IEDFSearch
    {
        SearchResult PerformSearch(string searchterm);
        SearchResult GetResultPage(string uri);
    }

    public interface IEDFList
    {
        SearchResult GetList(string list, string searchpage = "");
        IEnumerable<string> AvailableLists { get; }
    }

    public interface IEDFContentProvider
    {
        ParseResult ParsePage(Bookmark bm);
    }

    public interface IEDFPluginOptions
    {
        object Options { get; set; }
    }

}
