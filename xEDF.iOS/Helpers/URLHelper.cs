using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace xEDF.iOS.Helpers
{
    public class URLHelper
    {
        private static string[] urlSchemes = new string[] { "infuse://x-callback-url/play?url={0}", "vlc-x-callback://x-callback-url/stream?url={0}" };
        private static string[] urlSchemesAppName = new string[] { "InFuse", "VLC" };

        public static List<KeyValuePair<string, string>> GetAvailableOpenWithURLs(string link)
        {
            List<KeyValuePair<string, string>> available = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < urlSchemes.Length; i++)
            {
                string formatted = string.Format(urlSchemes[i], link);
                if (UIApplication.SharedApplication.CanOpenUrl(new Foundation.NSUrl(formatted)))
                    available.Add(new KeyValuePair<string, string>(urlSchemesAppName[i], formatted));                    
            }

            return available;
        }
    }
}
