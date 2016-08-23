using System;
using System.Collections.Generic;
using System.Text;
using xEDFlib.Helpers;

namespace xEDF.Droid
{
    public class Repo
    {
        public static RDHelper RDHelper { get; set; }
        public static xSettings Settings { get; set; }
        public static xEDFDroidUpdater Updater { get; set; }

        static Repo()
        {
            Settings = new xSettings(xEDFDroidApplication.Context);
            Updater = new xEDFDroidUpdater(xEDFDroidApplication.Context, xEDFDroidApplication.Version, Constants.ReleaseManifestUrl);
            RDHelper = new RDHelper(Settings);
        }
    }
}
