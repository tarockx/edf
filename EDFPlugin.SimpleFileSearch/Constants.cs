using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.SimpleFileSearch
{
    public static class Constants
    {
        public static string SimpleFoleSearchBaseHostNoTrailingSlash { get { return "http://www.simplefilesearch.com"; } }
        public static string SimpleFoleSearchSearchUrl { get { return "http://www.simplefilesearch.com/search.html?q=$searchterm$"; } }
        
    }
}
