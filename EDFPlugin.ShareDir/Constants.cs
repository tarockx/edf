using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.ShareDir
{
    public static class Constants
    {
        public static string ShareDirBaseHostNoTrailingSlash { get { return "http://sharedir.com"; } }
        public static string ShareDirSearchSearchUrl { get { return "http://sharedir.com/index.php?s=$searchterm$"; } }
        
    }
}
