using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.Filespr
{
    public static class Constants
    {
        public static string FilesprBaseHostNoTrailingSlash { get { return "http://www.filespr.net"; } }
        public static string FilesprSearchUrl { get { return "http://www.filespr.net/$searchinitial$/$searchterm$"; } }
        
    }
}
