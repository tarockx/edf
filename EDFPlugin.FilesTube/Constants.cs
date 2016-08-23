using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.FilesTube
{
    public static class Constants
    {
        public static string FilestTubeBaseHostNoTrailingSlash { get { return "http://www.filestube.to"; } }
        public static string FilesTubeSearchUrl { get { return "http://www.filestube.to/file/query.html?q=$searchterm$&select=$extension$"; } }
        
    }
}
