using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealDebrid4DotNet
{
    public static class Constants
    {
        public static string RDDeviceEndpoint { get { return "https://api.real-debrid.com/oauth/v2/device/code?client_id={0}&new_credentials=yes"; } }
        public static string RDCredentialsEndpoint { get { return "https://api.real-debrid.com/oauth/v2/device/credentials?client_id={0}&code={1}"; } }
        public static string RDTokenEndpoint { get { return "https://api.real-debrid.com/oauth/v2/token"; } }

        public static string RDUnrestrictLink { get { return "https://api.real-debrid.com/rest/1.0/unrestrict/link?auth_token={0}"; } }
        public static string RDSupportedHostsEndpoint { get { return "https://api.real-debrid.com/rest/1.0/hosts/domains?auth_token={0}"; } }

        public static string Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
    }
}
