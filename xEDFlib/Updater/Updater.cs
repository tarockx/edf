using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace xEDFlib.Updater
{
    public class Updater
    {
        protected string _currentVersion;
        protected string _releaseManifestUrl;

        public Updater(string currentVersion, string releaseManifestUrl)
        {
            _currentVersion = currentVersion;
            _releaseManifestUrl = releaseManifestUrl;
        }

        public Task<CheckForUpdatesResponse> CheckForUpdatesAsync()
        {
            var task = Task.Factory.StartNew(() => CheckForUpdates());
            return task;
        }

        public CheckForUpdatesResponse CheckForUpdates()
        {
            CheckForUpdatesResponse resp = new CheckForUpdatesResponse(_currentVersion);

            try
            {
                using (WebClient Client = new WebClient())
                {
                    var datastream = new MemoryStream(Client.DownloadData(_releaseManifestUrl));

                    XmlSerializer serializer = new XmlSerializer(typeof(ReleaseInfoCollection));

                    var manifest = (ReleaseInfoCollection)serializer.Deserialize(datastream);
                    datastream.Close();

                    resp.ReleaseManifest = manifest.releases;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return resp;
        }
        
    }
}
