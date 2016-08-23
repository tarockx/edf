using System;
using System.Xml.Serialization;

namespace libEraDeiFessi
{
    [Serializable]
    [XmlRoot(Namespace = "libEraDeiFessi")]
    [XmlInclude(typeof(Bookmark))]
    public class Bookmark
    {
        public string PluginID { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Subtitle { get { return (string.IsNullOrEmpty(Description) ? Url : Description); } }
        [XmlIgnore]public LinkMetadata Metadata { get; set; }
        public bool HasMetadata { get { return Metadata != null; } }

        public Bookmark() { } //For serialization purposes ONLY!
        public Bookmark(string pluginID) { PluginID = pluginID; }
        public Bookmark(string pluginID, string showname, string url) { PluginID = pluginID; Name = showname; Url = url; }
        public Bookmark(string pluginID, string showname, string url, string desc) { PluginID = pluginID; Name = showname; Url = url; Description = desc; }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Bookmark p = (Bookmark)obj;
            return Url.Equals(p.Url) && PluginID.Equals(p.PluginID);
        }
    }
}
