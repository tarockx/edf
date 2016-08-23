using System;
using System.Reflection;

namespace EraDeiFessi
{
    public static class Constants
    {
        public static string PotPlayerRegKey { get { return "Software\\Daum\\PotPlayer"; } }
        public static string PotPlayerRegKey64 { get { return "Software\\Daum\\PotPlayer64"; } }
        public static string PotPlayerMiniRegKey64 { get { return "Software\\Daum\\PotPlayerMini64"; } }
        public static string PotPlayerPathEntry { get { return "ProgramPath"; } }

        public static string VLCRegKey { get { return "Software\\VideoLAN\\VLC"; } }
        public static string VLCRegKey64 { get { return "Software\\Wow6432Node\\VideoLAN\\VLC"; } }
        
        public static string IDMRegKey { get { return "Software\\DownloadManager"; } }
        public static string IDMPathEntry { get { return "ExePath"; } }

        public static string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        public static string Title { get { return "EraDeiFessi - v " + Version; } }
        public static string ProgramLocation { get { var exe = Assembly.GetExecutingAssembly().Location; return exe.Substring(0, exe.LastIndexOf("\\")); } }
        public static string ExtensionLocation { get { return ProgramLocation + "\\EraDeiFessi4Chrome.crx"; } }
        public static string ProgramExeLocation { get { var exe = Assembly.GetExecutingAssembly().Location; return exe; } }
        public static string PluginsLocation { get { return ProgramLocation + "\\plugins"; } }
        public static string EDFAppdataFolder { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EraDeiFessi"; } }
        public static string UpdateAppdataFolder { get {return EDFAppdataFolder + "\\update"; } }
        public static string TorrentDownloadFolder { get { return Repository.Repo.Settings.TorrentDownloadPath + "\\EDFTorrent"; } }


        public static string ReleaseManifestUrl { get { return "http://eradeifessi.altervista.org/downloads/releases.xml"; } }
        public static string LatestReleasePackageUrl { get { return "http://eradeifessi.altervista.org/downloads/latest.zip"; } }
        public static string edfAPIUrl { get { return "http://eradeifessi.altervista.org/edfAPI.php"; } }

    }
}
