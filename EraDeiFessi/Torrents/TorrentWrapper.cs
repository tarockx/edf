using Ragnar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EraDeiFessi.Torrents
{
    public class TorrentWrapper : INotifyPropertyChanged
    {
        public TorrentHandle handle;
        private TorrentStatus status;
        private string[] properties = { "Speed", "TotalBytesToDownload", "BytesDownloaded", "PercentDone", "Seeds", "Peers", "StateString", "Progress" };

        public string Link { get; set; }
        public int Speed { get { return status.DownloadRate; } } //bps
        public long TotalBytesToDownload { get { return status.TotalWanted; } }
        public long BytesDownloaded { get { return status.TotalWantedDone; } }
        public double PercentDone { get { double perc = Math.Round(((double)BytesDownloaded / (double)TotalBytesToDownload) * 100, 1); return double.IsNaN(perc) ? 0 : perc; } }
        public double Progress { get { return Math.Round(status.Progress * 100.0, 1); } }
        public int Seeds { get { return status.NumSeeds; } }
        public int Peers { get { return status.NumPeers; } }
        public string StateString
        {
            get
            {
                switch (status.State)
                {
                    case TorrentState.Allocating:
                        return "allocazione spazio su disco";
                    case TorrentState.CheckingFiles:
                        return "controllo file";
                    case TorrentState.CheckingResumeData:
                        return "ripristino sessione";
                    case TorrentState.Downloading:
                        return "download";
                    case TorrentState.DownloadingMetadata:
                        return "recupero informazioni sul torrent";
                    case TorrentState.Finished:
                        return "completato";
                    case TorrentState.QueuedForChecking:
                        return "in coda";
                    case TorrentState.Seeding:
                        return "completato e in seed";
                    default:
                        return "N/A";
                }
            }
        }
        public TorrentState State { get { return status.State; } }

        public string DownloadDirectory { get { return status != null ? status.SavePath : ""; } }
        public string DownloadSubDirectory
        {
            get
            {
                try
                {
                    return DownloadDirectory + "\\" + handle.TorrentFile.Name;
                }
                catch
                {
                    return null;
                }
            }
        }

        public TorrentWrapper(TorrentHandle handle, string originalURL)
        {
            Link = originalURL;
            this.handle = handle;
            status = handle.GetStatus();
        }

        public void ReQuery()
        {
            try
            {
                status = handle.GetStatus();
                foreach (var property in properties)
                {
                    OnPropertyChanged(property);
                }
            }
            catch { }
        }


        public Task DownloadSingleFileAsync(FileEntry entry)
        {
            var task = Task.Factory.StartNew(() => DownloadSingleFile(entry));
            return task;
        }
        public void DownloadSingleFile(FileEntry entry)
        {
            TorrentInfo info = handle.TorrentFile;

            //for (int i = 0; i < info.NumFiles; i++)
            //{
            //    FileEntry currentEntry = info.FileAt(i);
            //    if (currentEntry.Path == entry.Path)
            //        handle.SetFilePriority(i, 7);
            //    else
            //        handle.SetFilePriority(i, 0);
            //}

            //handle.SequentialDownload = true;

            for (int i = 0; i < info.NumPieces; i++)
            {
                long currentOffset = (long)i * (long)(info.PieceLength);
                if (currentOffset + info.PieceLength < entry.Offset || currentOffset - info.PieceLength > entry.Offset + entry.Size)
                    handle.SetPiecePriority(i, 0);
                //else
                //handle.SetPiecePriority(i, 7);
            }
        }


        public FileEntry GetBiggestVideoFile()
        {
            try
            {
                TorrentInfo info = handle.TorrentFile;
                FileEntry currentEntry = null;
                for (int i = 0; i < info.NumFiles; i++)
                {
                    FileEntry entry = info.FileAt(i);
                    string ext = entry.Path.ToLower();
                    if (ext.EndsWith(".avi") || ext.EndsWith(".mkv") || ext.EndsWith(".mp4") || ext.EndsWith(".divx") || ext.EndsWith(".vmw") || ext.EndsWith(".flv"))
                    {
                        if (currentEntry == null || currentEntry.Size < entry.Size)
                            currentEntry = entry;
                    }
                }

                return currentEntry;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<FileEntry> GetAllVideoFiles()
        {
            List<FileEntry> list = new List<FileEntry>();
            try
            {
                TorrentInfo info = handle.TorrentFile;
                for (int i = 0; i < info.NumFiles; i++)
                {
                    FileEntry entry = info.FileAt(i);
                    string ext = entry.Path.ToLower();
                    if (ext.EndsWith(".avi") || ext.EndsWith(".mkv") || ext.EndsWith(".mp4") || ext.EndsWith(".divx") || ext.EndsWith(".vmw") || ext.EndsWith(".flv"))
                    {
                        list.Add(entry);
                    }
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
