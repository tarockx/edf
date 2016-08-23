using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace xEDF.Droid
{
    public class Constants
    {
        public static string ReleaseManifestUrl { get { return "http://eradeifessi.altervista.org/downloads/releases_android.xml"; } }
        public static string LatestReleasePackageUrl { get { return "http://eradeifessi.altervista.org/downloads/latest.apk"; } }
    }
}