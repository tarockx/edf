using Ragnar;
using System.Collections.ObjectModel;


namespace EraDeiFessi.Torrents
{
    public class TorrentsManager
    {
        private static Session currentSession;
        //public static ObservableCollection<TorrentWrapper> Torrents { get; set; } = new ObservableCollection<TorrentWrapper>();
        public static TorrentWrapper CurrentTorrent { get; set; }

        public static void CreateSession()
        {
            currentSession = new Session();
            currentSession.ListenOn(Repository.Repo.Settings.TorrentDownloadPort, Repository.Repo.Settings.TorrentDownloadPort + 9);
            currentSession.StartUpnp();
            currentSession.StartNatPmp();
        }

        public static TorrentWrapper StartTorrent(string magnetLink, string downloadPath)
        {
            try
            {
                if (!System.IO.Directory.Exists(downloadPath))
                    System.IO.Directory.CreateDirectory(downloadPath);
                if (!System.IO.Directory.Exists(downloadPath))
                {
                    return null;
                }


                //Check session
                if (currentSession == null)
                    CreateSession();

                // Create the AddTorrentParams with info about the torrent
                var addParams = new AddTorrentParams
                {
                    SavePath = downloadPath,
                    Url = magnetLink,
                };

                //Remove existing torrent, if any
                if (CurrentTorrent != null)
                {
                    currentSession.RemoveTorrent(CurrentTorrent.handle, true);
                }

                // Add a torrent to the session
                var handle = currentSession.AddTorrent(addParams);
                handle.SequentialDownload = true;
                TorrentWrapper torrentWrapper = new TorrentWrapper(handle, magnetLink);
                CurrentTorrent = torrentWrapper;

                return torrentWrapper;

            }
            catch (System.Exception)
            {
                return null;
            }

        }

        public static TorrentWrapper StartTorrentFromFile(string filePath, string downloadPath)
        {
            try
            {
                if (!System.IO.Directory.Exists(downloadPath))
                    System.IO.Directory.CreateDirectory(downloadPath);

                //Check session
                if (currentSession == null)
                    CreateSession();

                // Create the AddTorrentParams with info about the torrent
                var addParams = new AddTorrentParams
                {
                    SavePath = downloadPath,
                    TorrentInfo = new TorrentInfo(filePath),
                };

                //Remove existing torrent, if any
                if (CurrentTorrent != null)
                {
                    currentSession.RemoveTorrent(CurrentTorrent.handle, true);
                }

                // Add a torrent to the session
                var handle = currentSession.AddTorrent(addParams);
                handle.SequentialDownload = true;
                TorrentWrapper torrentWrapper = new TorrentWrapper(handle, null);
                CurrentTorrent = torrentWrapper;

                return torrentWrapper;

            }
            catch (System.Exception)
            {
                return null;
            }

        }

        public static void RemoveTorrent(bool removeData)
        {
            if (CurrentTorrent != null)
            {
                if (currentSession != null)
                    currentSession.RemoveTorrent(CurrentTorrent.handle, removeData);

                CurrentTorrent = null;
            }
        }
        

    }
}
