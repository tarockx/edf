using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.DownloadNow
{
    public static class Constants
    {
        public static string DownloadNowBaseHostNoTrailingSlash { get { return "http://www.downloadnow.net"; } }
        public static string DownloadNowSearchUrl { get { return "http://www.downloadnow.net/d/$searchterm$"; } }
        
    }
}
