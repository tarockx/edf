using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using RealDebrid4DotNet.RestModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EraDeiFessi.Repository
{

    [XmlRoot(Namespace = "EraDeiFessi.Repository")]
    [XmlInclude(typeof(SettingsRepo))]
    [Serializable()]
    public class SettingsRepo
    {
        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public string PathToPotplayerExe { get; set; }
        public string PathToVLCExe { get; set; }
        public string PathToIDMExe { get; set; }
        public int ZoomIndex { get; set; }
        public int DefaultTab { get; set; }
        public TokenRequestResponse RDToken { get; set; }
        public bool EnableExtensionService { get; set; }
        public bool OnLinkSentDownloadImmediately { get; set; }
        public bool CallUpgrade { get; set; }
        public bool CheckForUpdates { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool UseGoogleSuggestions { get; set; }
        public bool TrackHistory { get; set; }

        public bool UseSharedHistory { get; set; }
        public string EDFAccount_Username { get; set; }
        public string EDFAccount_Password { get; set; }

        public bool SearchAllPlugins { get; set; }
        public string[] SelectedSearchPlugins { get; set; }

        public string TorrentDownloadPath { get; set; }
        public int TorrentDownloadPort { get; set; }

        public SerializableDictionary<string, HistoryEntry> AccessHistory { get; set; }
        public PluginOptionsRepo PluginOptions {get; set;}
        
        public SettingsRepo()
        {
            //Default settings! Will be overridden after first use!
            Bookmarks = new ObservableCollection<Bookmark>();
            CallUpgrade = true;
            CheckForUpdates = true;
            EnableExtensionService = false;
            MinimizeToTray = false;
            UseGoogleSuggestions = true;
            SearchAllPlugins = true;
            OnLinkSentDownloadImmediately = false;
            TrackHistory = true;
            UseSharedHistory = false;
            EDFAccount_Password = EDFAccount_Username = string.Empty;
            AccessHistory = new SerializableDictionary<string, HistoryEntry>();
            TorrentDownloadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            TorrentDownloadPort = 41222;
            PluginOptions = new PluginOptionsRepo();
        }
    }
}
