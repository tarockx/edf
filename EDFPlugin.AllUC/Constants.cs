using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.AllUC
{
    public static class Constants
    {
        public static string AllUCBaseHostNoTrailingSlash { get { return "http://www.alluc.com"; } }
        public static string AllUCDownloadSearchSearchUrl { get { return "http://www.alluc.com/download/$searchterm$"; } }
        public static string AllUCStreamSearchSearchUrl { get { return "http://www.alluc.com/stream/$searchterm$"; } }
        
    }
}
