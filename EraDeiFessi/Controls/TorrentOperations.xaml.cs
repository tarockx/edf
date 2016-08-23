using EraDeiFessi.Repository;
using EraDeiFessi.Torrents;
using Ragnar;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using System.Windows.Threading;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for TorrentOperations.xaml
    /// </summary>
    public partial class TorrentOperations : UserControl
    {
        private Timer queryTimer;
        private Dispatcher UIDispatcher;
        private string selectedFile;
        private bool PotPlayerAvailable;
        private bool VLCAvailable;
        TaskbarItemInfo taskBarInfo;

        public TorrentWrapper Torrent { get; set; }
        public ObservableCollection<FileEntry> AvailableFiles { get; set; }

        

        public TorrentOperations(TorrentWrapper wrapper, TaskbarItemInfo taskBarInfo)
        {
            this.taskBarInfo = taskBarInfo;
            Torrent = wrapper;
            UIDispatcher = Dispatcher;
            AvailableFiles = new ObservableCollection<FileEntry>();
            PotPlayerAvailable = System.IO.File.Exists(Repo.Settings.PathToPotplayerExe);
            VLCAvailable = System.IO.File.Exists(Repo.Settings.PathToVLCExe);

            InitializeComponent();

            groupOriginalLink.IsEnabled = wrapper.Link != null;

            Torrent.ReQuery();
            UpdateUI();

            queryTimer = new Timer();
            queryTimer.Elapsed += QueryTimer_Elapsed;
            queryTimer.Interval = 2000;
            queryTimer.Enabled = true;
        }

        private void QueryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Torrent.ReQuery();

            UIDispatcher.Invoke(UpdateUI, DispatcherPriority.Normal);
        }

        public void Stop()
        {
            queryTimer.Stop();
        }
        
        private void UpdateUI()
        {
            panelLoading.Visibility = Visibility.Collapsed;
            gridDownloading.Visibility = Visibility.Collapsed;
            panelError.Visibility = Visibility.Collapsed;
            panelFileSelection.Visibility = Visibility.Collapsed;
            panelStatus.Visibility = AvailableFiles.Count == 0 ? Visibility.Hidden : Visibility.Visible;
            groupNoVideoInfo.Visibility = AvailableFiles.Count == 0 ? Visibility.Visible : Visibility.Hidden;

            tbStatus_Downloading.Visibility = Visibility.Collapsed;
            tbStatus_Preparing.Visibility = Visibility.Collapsed;
            tbStatus_Error.Visibility = Visibility.Collapsed;
            tbStatus_Finished.Visibility = Visibility.Collapsed;

            bool readyToStream = false;

            switch (Torrent.State)
            {
                case TorrentState.Allocating:
                case TorrentState.CheckingFiles:
                case TorrentState.CheckingResumeData:
                case TorrentState.QueuedForChecking:
                case TorrentState.DownloadingMetadata:
                    if(taskBarInfo != null)
                        taskBarInfo.ProgressState = TaskbarItemProgressState.Paused;
                    panelLoading.Visibility = Visibility.Visible;
                    tbStatus_Preparing.Visibility = Visibility.Visible;
                    break;
                case TorrentState.Downloading:
                    tbStatus_Downloading.Visibility = Visibility.Visible;
                    if (selectedFile == null) //Still to chose video file
                    {
                        checkFileSelectionStatus();
                    }
                    else //File already chosen, show download panel
                    {
                        gridDownloading.Visibility = Visibility.Visible;
                        readyToStream = Torrent.PercentDone >= 1.0;
                        if (taskBarInfo != null)
                        {
                            taskBarInfo.ProgressState = TaskbarItemProgressState.Normal;
                            taskBarInfo.ProgressValue = Torrent.PercentDone / 100.0;
                        }
                    }
                    break;
                case TorrentState.Finished:
                case TorrentState.Seeding:
                    tbStatus_Finished.Visibility = Visibility.Visible;
                    if (taskBarInfo != null)
                        taskBarInfo.ProgressState = TaskbarItemProgressState.None;
                    if (selectedFile == null) //Still to chose video file
                    {
                        checkFileSelectionStatus();
                    }
                    else //File already chosen, show download panel
                    {
                        gridDownloading.Visibility = Visibility.Visible;
                        readyToStream = true;
                    }
                    break;
                default:
                    if (taskBarInfo != null)
                        taskBarInfo.ProgressState = TaskbarItemProgressState.Error;
                    tbStatus_Error.Visibility = Visibility.Visible;
                    panelError.Visibility = Visibility.Visible;
                    lblError.Text = "Unknown error";
                    break;
            }

            btnRDStreamPot.IsEnabled = readyToStream && PotPlayerAvailable && selectedFile != null && System.IO.File.Exists(selectedFile);
            btnRDStreamVLC.IsEnabled = readyToStream && VLCAvailable && selectedFile != null && System.IO.File.Exists(selectedFile);
        }

        private void checkFileSelectionStatus()
        {
            var videoFiles = Torrent.GetAllVideoFiles();
            if (AvailableFiles.Count == 0)
                foreach (var item in videoFiles)
                {
                    AvailableFiles.Add(item);
                }

            if (videoFiles.Count == 0) //No video files in this torrent
            {
                gridDownloading.Visibility = Visibility.Visible;
            }
            else if (videoFiles.Count == 1) //Only one video file, automatic selection
            {
                selectedFile = Torrent.DownloadDirectory + @"\" + videoFiles[0].Path;
            }
            else //Multiple video files, show selection panel
            {
                panelFileSelection.Visibility = Visibility.Visible;
            }
        }

        private void btnRDStreamPot_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Repo.Settings.PathToPotplayerExe))
            {
                string path = Repo.Settings.PathToPotplayerExe;
                Process.Start(path, selectedFile);
            }
        }

        private void btnRDStreamVLC_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Repo.Settings.PathToVLCExe))
            {
                string path = Repo.Settings.PathToVLCExe;
                Process.Start(path, "-vvv \"" + selectedFile + "\"");
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(Torrent.Link);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }
        }

        private void btnOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Torrent.Link);
        }

        private async void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            comboFiles.IsEnabled = false;
            btnSelectFile.IsEnabled = false;

            FileEntry entry = comboFiles.SelectedItem as FileEntry;

            await Torrent.DownloadSingleFileAsync(entry);
                        
            selectedFile = Torrent.DownloadDirectory + @"\" + entry.Path;
        }

        private void comboFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnSelectFile.IsEnabled = comboFiles.SelectedItem != null;
        }
    }
}
