using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using libEraDeiFessi;
using RealDebrid4DotNet;

namespace EraDeiFessi.Repository
{
    static class Repo
    {
        public static SettingsRepo Settings { get; set; }
        public static RDAgent RDAgent { get; set; }

        static Repo()
        {
            LoadSettings();

            if (Settings.RDToken != null && Settings.RDToken.Valid)
                RDAgent = new RDAgent(Settings.RDToken);
            else
                RDAgent = new RDAgent(libEraDeiFessi.Constants.RealDebridEDFClientID);

            if (Settings.Bookmarks == null)
                Settings.Bookmarks = new System.Collections.ObjectModel.ObservableCollection<Bookmark>();
            SaveSettings();

        }

        public static void LogoutRD()
        {
            Settings.RDToken = null;
            RDAgent.Logout();
            SaveSettings();
        }


        public static void SaveSettings()
        {
            Settings.CallUpgrade = false;
            if(RDAgent != null && RDAgent.Token != null && RDAgent.Token.Valid)
            {
                Settings.RDToken = RDAgent.Token;
            }

            MySettings.Default.SettingsRepo = Settings;
            MySettings.Default.Save();
        }

        public static void LoadSettings()
        {
            if (MySettings.Default.SettingsRepo == null || MySettings.Default.SettingsRepo.CallUpgrade)
            {
                MySettings.Default.Upgrade();
                MySettings.Default.Reload();
            }
            

            Settings = MySettings.Default.SettingsRepo;
            if (Settings == null)
            {
                Settings = new SettingsRepo();
                SaveSettings();
            }

            if (!File.Exists(Settings.PathToPotplayerExe))
            {
                string path = Utilities.GetRegistryKey(Constants.PotPlayerRegKey, Constants.PotPlayerPathEntry, Microsoft.Win32.RegistryHive.CurrentUser);
                if(string.IsNullOrWhiteSpace(path))
                    path = Utilities.GetRegistryKey(Constants.PotPlayerRegKey64, Constants.PotPlayerPathEntry, Microsoft.Win32.RegistryHive.CurrentUser);
                if (string.IsNullOrWhiteSpace(path))
                    path = Utilities.GetRegistryKey(Constants.PotPlayerMiniRegKey64, Constants.PotPlayerPathEntry, Microsoft.Win32.RegistryHive.CurrentUser);
                Settings.PathToPotplayerExe = path;
                SaveSettings();
            }

            if (!File.Exists(Settings.PathToVLCExe))
            {
                string path = Utilities.GetRegistryKey(Constants.VLCRegKey, null, Microsoft.Win32.RegistryHive.LocalMachine);
                if (string.IsNullOrWhiteSpace(path))
                    path = Utilities.GetRegistryKey(Constants.VLCRegKey64, null, Microsoft.Win32.RegistryHive.LocalMachine);
                Settings.PathToVLCExe = path;
                SaveSettings();
            }

            if (!File.Exists(Settings.PathToIDMExe))
            {
                string path = Utilities.GetRegistryKey(Constants.IDMRegKey, Constants.IDMPathEntry, Microsoft.Win32.RegistryHive.CurrentUser);
                Settings.PathToIDMExe = path;
                SaveSettings();
            }

        }

        public static bool IsBookmarkAlreadyPresent(Bookmark s)
        {
            foreach (var item in Settings.Bookmarks)
            {
                if (item.Equals(s))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<Bookmark> GetBookmarksByPlugin(string pluginID)
        {
            try
            {
                return (from b in Settings.Bookmarks where b.PluginID.Equals(pluginID) select b);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
