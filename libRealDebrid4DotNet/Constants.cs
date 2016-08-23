using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace libRealDebrid4DotNet
{
    public static class Constants
    {
        public static string RealDebridUnrestrictLinkWithCookie { get { return "https://real-debrid.com/ajax/unrestrict.php?link={0}"; } }
        public static string RealDebridUnrestrictLinkWithLogin { get { return "https://real-debrid.com/ajax/unrestrict.php?link={0}&user={1}&pass={2}"; } }
        public static string RealDebridAuthenticationLink { get { return "http://real-debrid.com/ajax/login.php?user={0}&pass={1}"; } }
        public static string RealDebridHostListLink { get { return "https://real-debrid.com/api/hosters.php"; } }

        public static string Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
    }
}
