using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace xEDFlib.Updater
{
    [Serializable()]
    public class CheckForUpdatesResponse
    {
        private string _currentVersion;

        public CheckForUpdatesResponse(string currentVersion)
        {
            _currentVersion = currentVersion;
        }

        public List<ReleaseInfo> ReleaseManifest { get; set; }
        public bool IsNewVersionAvailable
        {
            get
            {
                try
                {
                    var res = ReleaseManifest.First();
                    foreach (var item in ReleaseManifest)
                    {
                        if (item.IsNewer(res.Version))
                            res = item;
                    }

                    var latestVer = res.Version.Split('.');
                    var currentVer = _currentVersion.Split('.');

                    for (int i = 0; i < latestVer.Count(); i++)
                    {
                        if (int.Parse(latestVer[i]) > int.Parse(currentVer[i]))
                            return true;
                        if (int.Parse(latestVer[i]) < int.Parse(currentVer[i]))
                            return false;
                    }
                    return false;
                        //return res.Version.CompareTo(Constants.Version) > 0;
                }
                catch
                { return false; }
            }
        }

        public ReleaseInfo GetNewVersionInfo()
        {
            try
            {
                var res = ReleaseManifest.First();
                foreach (var item in ReleaseManifest)
                {
                    if (item.IsNewer(res.Version))
                        res = item;
                }
                return res;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
