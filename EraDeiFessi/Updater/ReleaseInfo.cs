using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EraDeiFessi.Updater
{
    [Serializable()]
    public class ReleaseInfo
    {
        [XmlElement()]
        public string Version { get; set; }
        [XmlElement()]
        public string Notes { get; set; }
        [XmlAttribute()]
        public bool IsLatest { get; set; }

        public ReleaseInfo() { IsLatest = false; }

        public bool IsNewer(string version)
        {
            var myVersion = Version.Split('.');
            var rVer = version.Split('.');

            for (int i = 0; i < myVersion.Count(); i++)
            {
                if (int.Parse(myVersion[i]) > int.Parse(rVer[i]))
                    return true;
                if (int.Parse(myVersion[i]) < int.Parse(rVer[i]))
                    return false;
            }
            return false;
        }
    }

    //[XmlRoot(ElementName = "releases")]
    public class ReleaseInfoCollection
    {
       [XmlElement("release")] public List<ReleaseInfo> releases { get; set; }
    }
}
