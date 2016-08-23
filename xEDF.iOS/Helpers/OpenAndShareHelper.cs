using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS.Helpers
{
    public class OpenAndShareHelper
    {
        public static void OpenUrl(string url)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
        }
    }
}
