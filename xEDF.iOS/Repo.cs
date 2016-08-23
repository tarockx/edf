using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using xEDFlib.Helpers;

namespace xEDF.iOS
{
    public class Repo
    {
        public static RDHelper RDHelper { get; set; }
        public static xSettings Settings { get; set; }
        public static xEDFiOSUpdater Updater { get; set; }

        static Repo()
        {
            Settings = new xSettings();
            Updater = new xEDFiOSUpdater(NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString());
            RDHelper = new RDHelper(Settings);
        }
    }
}
