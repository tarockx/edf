using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using EraDeiFessi.Repository;
using EraDeiFessi.Controls;
using System.Threading;
using Microsoft.Win32;
using libEraDeiFessi.Plugins;
using libEraDeiFessi;
using System.Reflection;
using EraDeiFessi.Webservice;
using System.Windows.Controls.Primitives;
using PropGrid4WPF;
using System.Diagnostics;
using EraDeiFessi.Helpers;
using EraDeiFessi.Torrents;
using log4net;

namespace EraDeiFessi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ILog Logger { get; private set; } = LogManager.GetLogger(typeof(MainWindow));

        #region "Binding data"

        public Bookmark CurrentContent { get; set; }
        public ParseResult.ParseResultType CurrentContentType { get; set; }

        private List<IEDFSearch> EnabledSearchPlugins = new List<IEDFSearch>();

        public Helpers.GoogleSuggestionsProvider SuggestionsProvider { get; set; }
        #endregion

        #region Window events

        public MainWindow()
        {
            try
            {
                Logger.Debug("Initializing Google suggestions provider");
                SuggestionsProvider = new Helpers.GoogleSuggestionsProvider();

                Logger.Debug("Initializing MainWindow's XAML components");
                InitializeComponent();

                Logger.Debug("Hooking events");
                BookmarkManager.BookmarkSelected += new BookmarkManager.BookmarkSelectedEventHandler(GoToBookmark);
                BookmarkManager.BookmarkDeleted += new BookmarkManager.BookmarkDeletedEventHandler(DeletedBookmark);
                BookmarkManager.GetMoreResults += new BookmarkManager.GetMoreResultsEventHandler(GetMoreResults);
                BookmarkListControl.BookmarkSelected += new BookmarkListControl.BookmarkSelectedEventHandler(GoToBookmark);
                EDFServices.RequestReceived += new EDFServices.RequestReceivedEventHandler(UnblockRequestIncoming);
                HistoryManager.HistoryUpdated += new HistoryManager.HistoryUpdatedEventHandler(UpdateHistory);
                HistoryManager.HistoryModeToggled += new HistoryManager.HistoryModeToggledEventHandler(RecheckHistoryMode);

                Logger.Debug("Loading saved bookmarks");
                LoadBookmarks();


                var searchplugins = (from p in PluginsRepo.Plugins.Values where p is IEDFSearch select p as IEDFSearch).ToList();
                if (searchplugins != null)
                    icSearchPlugins.ItemsSource = searchplugins;

                var listplugins = from p in PluginsRepo.Plugins.Values where p is IEDFList select p as IEDFList;
                if (listplugins != null)
                    groupListe.Content = new BookmarkListControl(listplugins);

                //start the webservice, if enabled
                if (Repo.Settings.EnableExtensionService)
                {
                    Logger.Debug("Starting EDF webservice");
                    ServicesManager.StartListening();
                }


            }
            catch (Exception ex)
            {
                Logger.Debug("Exception in MainWindow()", ex);
                if (App.SHOW_DEBUG_MESSAGES)
                    MessageBox.Show("Eccezione non gestita:\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Eccezione non gestita", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    throw ex;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Repo.Settings.CheckForUpdates)
                {
                    Logger.Debug("Checking for updates...");
                    CheckForUpdates();
                }

                Logger.Debug("Setting up UI...");
                SetEnabledControls();
                lpHistoryToggle.Size = 12;


                comboZoom.SelectedItem = comboZoom.Items.GetItemAt(Repo.Settings.ZoomIndex);
                comboDefaultTab.SelectedItem = comboDefaultTab.Items.GetItemAt(Repo.Settings.DefaultTab);

                SetZoomFromComboBox();
                SetDefaultTabFromComboBox();

                Logger.Debug("Checking credentials and logging into RealDebrid");
                Repo.RDAgent.TokenRefreshed += (trsender, tre) => { RecheckRD(); };
                RecheckRD();

                Logger.Debug("Restoring settings");
                chkCheckForUpdates.IsChecked = Repo.Settings.CheckForUpdates;
                chkEnableExtensionService.IsChecked = Repo.Settings.EnableExtensionService;
                chkMinimizeToTray.IsChecked = Repo.Settings.MinimizeToTray;
                chkUseGoogleSuggestions.IsChecked = Repo.Settings.UseGoogleSuggestions;
                if (Repo.Settings.UseGoogleSuggestions)
                {
                    actb.Provider = SuggestionsProvider;
                    //mySubtitleSearch.actb.Provider = SuggestionsProvider;
                }
                else
                {
                    actb.Provider = null;
                    //mySubtitleSearch.actb.Provider = null;
                }

                if (Repo.Settings.OnLinkSentDownloadImmediately)
                    rbOnLinkDownload.IsChecked = true;
                else
                    rbOnLinkUnblock.IsChecked = true;

                chkSearchAll.IsChecked = Repo.Settings.SearchAllPlugins;
                chkTrackHistory.IsChecked = Repo.Settings.TrackHistory;
                if (!Repo.Settings.TrackHistory)
                    HistoryManager.ClearLocalHistory();
                setupSharedHistoryUI();

                txtTorrentDownloadPath.Text = Repo.Settings.TorrentDownloadPath;
                txtPortNumber.Value = Repo.Settings.TorrentDownloadPort;

                dynamic activeX = browserTorrentDescription.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, browserTorrentDescription, new object[] { });

                activeX.Silent = true;
            }
            catch (Exception ex)
            {
                Logger.Debug("Exception in MainWindow.Window_Loaded()", ex);
                if (App.SHOW_DEBUG_MESSAGES)
                    MessageBox.Show("Eccezione non gestita:\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Eccezione non gestita", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    throw ex;
            }

        }

        private void w1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool proceed = AskStopTorrentIfRunning(true);
            e.Cancel = !proceed;
        }

        #endregion

        #region History

        private async void TryEnableSharedHistory()
        {
            //chkSupportSharedHistory.IsChecked = Repo.Settings.UseSharedHistory;
            btnToggleSharedHsitory.Visibility = System.Windows.Visibility.Collapsed;
            lpHistoryToggle.Visibility = System.Windows.Visibility.Visible;

            bool res = await HistoryManager.DownloadSharedHistoryAsync();
            if (res)
            {
                HistoryManager.SharedModeActive = true;
            }
            else
            {
                HistoryManager.SharedModeActive = false;
                MessageBox.Show("Impossibile attivare la cronologia condivisa.\n\nIl tuo nome utente e password potrebbero essere errati o il sever EDF potrebbe non essere disponibile.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            btnToggleSharedHsitory.Visibility = System.Windows.Visibility.Visible;
            lpHistoryToggle.Visibility = System.Windows.Visibility.Collapsed;

            setupSharedHistoryUI();
            ReloadHistory();
        }

        private void DisableSharedHistory()
        {
            HistoryManager.SharedModeActive = false;
            setupSharedHistoryUI();
            ReloadHistory();
        }

        private void ReloadHistory()
        {
            ClearHistory();
            if (Repo.Settings.TrackHistory)
            {
                List<ContentNestBlock> cnbs = new List<ContentNestBlock>();
                foreach (var item in treeEpisodes.Items)
                {
                    cnbs.AddRange((item as NestBlock).GetContentBlocks());
                }
                foreach (var item in cnbs)
                {
                    var history = HistoryManager.GetHistory(from l in item.Links select l.Url);
                    if (history != null)
                        item.LastAccess = history.timestamp;
                }
            }
        }

        private void setupSharedHistoryUI()
        {
            chkSupportSharedHistory.IsChecked = Repo.Settings.UseSharedHistory;
            txt_EDFuser.Text = Repo.Settings.EDFAccount_Username;
            txt_EDFpass.Password = Repo.Settings.EDFAccount_Password;

            if (!Repo.Settings.TrackHistory || !Repo.Settings.UseSharedHistory)
                gridHistoryToggle.Visibility = System.Windows.Visibility.Collapsed;
            else
                gridHistoryToggle.Visibility = System.Windows.Visibility.Visible;

            if (HistoryManager.SharedModeActive)
            {
                //btnToggleSharedHsitory.Background = Brushes.Green;
                tbHistoryLocal.Visibility = System.Windows.Visibility.Collapsed;
                tbHistoryShared.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                //btnToggleSharedHsitory.Background = Brushes.Gray;
                tbHistoryLocal.Visibility = System.Windows.Visibility.Visible;
                tbHistoryShared.Visibility = System.Windows.Visibility.Collapsed;
            }

        }

        private void UpdateHistory(object sender, HistoryManager.HistoryUpdatedEventArgs e)
        {
            if (Repo.Settings.TrackHistory)
            {
                if (CurrentContentType == ParseResult.ParseResultType.NestedContent)
                {
                    var episode = treeEpisodes.SelectedItem as ContentNestBlock;
                    if (episode != null)
                    {
                        episode.LastAccess = e.Entry.timestamp;
                    }
                }
            }
        }

        private void btnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            ClearHistory();
            HistoryManager.ClearLocalHistory();
        }

        private void btnToggleSharedHistory_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryManager.SharedModeActive)
                DisableSharedHistory();
            else
                TryEnableSharedHistory();
        }

        private void RecheckHistoryMode(object sender, EventArgs e)
        {
            setupSharedHistoryUI();
            ReloadHistory();
        }

        #endregion

        #region Bookmarks

        private void DeletedBookmark(object sender, BookmarkManager.BookmarkDeletedEventArgs e)
        {
            if (CurrentContent != null && CurrentContent.Equals(e.Bookmark))
                SetBookmarkButtonsVisibility();

            if (((BookmarkManager)sender).Bookmarks.Count == 0)
                tcPreferiti.Items.Remove(((BookmarkManager)sender).Parent);
        }

        private void LoadBookmarks()
        {
            //Note: plugins must be already loaded before calling this!

            tcPreferiti.Items.Clear();
            List<Bookmark> whitelist = new List<Bookmark>();
            List<Bookmark> blacklist = new List<Bookmark>();

            foreach (var item in PluginsRepo.Plugins)
            {
                var bookmarks = Repo.GetBookmarksByPlugin(item.Key);
                if (bookmarks != null && bookmarks.Count() > 0)
                {
                    whitelist.AddRange(bookmarks);
                    BookmarkManager bmg = new BookmarkManager(bookmarks, true);
                    TabItem ti = new TabItem() { Header = item.Value.pluginName, Content = bmg };
                    tcPreferiti.Items.Add(ti);
                }
            }

            foreach (var item in Repo.Settings.Bookmarks)
            {
                if (!whitelist.Contains(item))
                    blacklist.Add(item);
            }

            foreach (var item in blacklist)
            {
                Repo.Settings.Bookmarks.Remove(item);
            }
        }

        private void btnAddToBookmarks_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentContent == null || Repo.IsBookmarkAlreadyPresent(CurrentContent))
                return;

            Repo.Settings.Bookmarks.Add(CurrentContent);
            LoadBookmarks();

            SetBookmarkButtonsVisibility();
        }


        private void btnDeleteBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentContent == null)
                return;

            TabItem toRemove = null;
            Repo.Settings.Bookmarks.Remove(CurrentContent);
            foreach (TabItem item in tcPreferiti.Items)
            {
                BookmarkManager bmg = (BookmarkManager)item.Content;
                bmg.Bookmarks.Remove(CurrentContent);
                if (bmg.Bookmarks.Count == 0)
                    toRemove = item;
            }

            if (toRemove != null)
                tcPreferiti.Items.Remove(toRemove);

            SetBookmarkButtonsVisibility();
        }

        private void goToBookmarksSection_Click(object sender, RoutedEventArgs e)
        {
            tcMain.SelectedItem = tabPreferiti;
        }

        #endregion

        #region Updater

        async private void CheckForUpdates()
        {
            var resp = await Updater.Updater.CheckForUpdatesAsync();

            if (resp != null && resp.IsNewVersionAvailable)
            {
                var rel = resp.GetNewVersionInfo();
                UpdateDialog dlg = new UpdateDialog(resp);
                dlg.Owner = this;
                var res = dlg.ShowDialog();
                if (!res.HasValue)
                    return;

                if (res.Value == false)
                {
                    if (dlg.DontAskAgain)
                    {
                        Repo.Settings.CheckForUpdates = false;
                        chkCheckForUpdates.IsChecked = false;
                        Repo.SaveSettings();
                    }
                }
                else
                {
                    gridUpdate.Visibility = System.Windows.Visibility.Visible;
                    bool updtDownloaded = await Updater.Updater.DownloadUpdateAsync();

                    if (updtDownloaded)
                    {
                        //update scaricata con successo
                        var rebootRes = MessageBox.Show("Aggiornamento scaricato con successo! Verrà installato automaticamente al prossimo avvio dell'applicazione.\n\nNOTA: Al momento dell'installazione ti verrà chiesto di fornire i permessi di amministrazione. Questo è necessario per la corretta installazione dell'aggiornamento\n\nRiavviare l'applicazione e installare l'aggiornamento ora?", "Riavvio richiesto", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (rebootRes == MessageBoxResult.Yes)
                        {
                            Updater.Updater.DecompressUpdater();
                            if (System.IO.File.Exists(Constants.UpdateAppdataFolder + "\\EDFUpdater.exe"))
                            {
                                Updater.Updater.InvokeUpdater();
                                this.Close();
                                Application.Current.Shutdown();
                            }
                            else
                            {
                                MessageBox.Show("Impossibile avviare l'aggiornamento automatico. Eseguibile di aggiornamento non trovato.", "File non trovato", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Impossibile scaricare l'aggiornamento. Il servizio di aggiornamento potrebbe essere momentaneamente non disponibile o potresti avere problemi con la tua connessione a internet. Si consiglia di riprovare più tardi.", "Download fallito", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    gridUpdate.Visibility = System.Windows.Visibility.Hidden;
                }
            }

        }

        private void pbDownload_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            pbDownload.IsIndeterminate = (pbDownload.Visibility == System.Windows.Visibility.Visible);
        }

        #endregion

        #region Change Content

        private void GoToBookmark(object sender, BookmarkManager.BookmarkSelectedEventArgs e)
        {
            GoToBookmark(e.Bookmark);
        }

        private void GoToBookmark(object sender, BookmarkListControl.BookmarkSelectedEventArgs e)
        {
            GoToBookmark(e.Bookmark);
        }

        private void GoToBookmark(Bookmark bm)
        {
            if (!AskStopTorrentIfRunning(false))
                return;

            ClearContent();
            ChangeContent(bm);
            tcMain.SelectedItem = tabContenuti;
        }

        async private void ChangeContent(Bookmark sb)
        {
            ShowLoadingPanels();
            CurrentContent = sb;
            CurrentContentType = ParseResult.ParseResultType.NestedContent; //hack...
            SetEnabledControls();

            ParseResult res = await ParsingHelpers.ParsePageAsync((IEDFContentProvider)PluginsRepo.Plugins[sb.PluginID], sb);

            if (res.HasError)
            {
                if (Repo.IsBookmarkAlreadyPresent(sb))
                {
                    var response = MessageBox.Show("E' stato impossibile recuperare il contenuto selezionato. Il link potrebbe essere non valido, il sito non raggiungibile o modificato o potresti non essere connesso a internet.\n\nQuesto contenuto è presente nei tuoi preferiti, desideri rimuoverlo?", "Caricamento fallito", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (response == MessageBoxResult.Yes)
                    {
                        Repo.Settings.Bookmarks.Remove(sb);
                        foreach (TabItem item in tcPreferiti.Items)
                        {
                            BookmarkManager bmg = (BookmarkManager)item.Content;
                            bmg.Bookmarks.Remove(sb);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("E' stato impossibile recuperare il contenuto selezionato. Il link potrebbe essere non valido, il sito non raggiungibile o modificato, oppure vi è un errore nel plugin o siete disconnessi da internet.", "Caricamento fallito", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                ClearContent();
            }
            else
            {
                CurrentContentType = res.ResultType;
                SetEnabledControls();

                switch (res.ResultType)
                {
                    case ParseResult.ParseResultType.HtmlContent:
                        ChangeHtmlContent(res.Result as HtmlContent);
                        break;
                    case ParseResult.ParseResultType.NestedContent:
                        ChangeNestedContent(res.Result as NestedContent);
                        break;
                    case ParseResult.ParseResultType.TorrentContent:
                        ChangeTorrentContent(res.Result as TorrentContent);
                        break;
                }
            }

            HideLoadingPanels();
        }

        private void ChangeNestedContent(NestedContent res)
        {
            treeEpisodes.ItemsSource = res.Children;

            ReloadHistory();

            if (!string.IsNullOrWhiteSpace(res.CoverImageUrl))
                SetCoverImageAsync(res.CoverImageUrl);

            descriptionGrid.Visibility = Visibility.Visible;

            lblSeasonCount.Text = res.Children.Count().ToString();
            int c = 0;
            foreach (var item in res.Children)
            {
                c += item.LinkCount();
            }

            lblEpisodeCount.Text = c.ToString();
            lblDescription.Text = res.Description;

            if (res.Children.Count == 1)
            {
                var p = res.Children[0];
                p.IsExpanded = true;
                if (p.Children.Count == 1)
                {
                    p.Children[0].IsSelected = true;
                }
            }
        }

        private void ChangeHtmlContent(HtmlContent res)
        {
            if (res != null && !string.IsNullOrEmpty(res.Content))
            {
                //browserMovieLinks.Navigating -= browserMovieLinks_Navigating;

                browserMovieLinks.NavigateToString(res.Content);
                //browserMovieLinks.DocumentText = res.Content;

                //browserMovieLinks.Navigating += browserMovieLinks_Navigating;

                if (!string.IsNullOrWhiteSpace(res.CoverImageUrl))
                    SetCoverImageAsync(res.CoverImageUrl);

                descriptionGrid.Visibility = System.Windows.Visibility.Visible;
                if (res.HideDescriptionPanel)
                {
                    groupInfoPanel.Visibility = Visibility.Collapsed;
                    groupLinkFilm.SetValue(Grid.ColumnSpanProperty, 2);
                }
                lblDescription.Text = res.Description;
                lblMovieTitle.Text = CurrentContent.Name;
            }
        }

        

        private void ChangeTorrentContent(TorrentContent res)
        {
            if (res != null && !string.IsNullOrEmpty(res.MagnetURI))
            {
                tbTorrentTitle.Text = res.Title;
                if (!string.IsNullOrEmpty(res.HtmlDescription))
                    browserTorrentDescription.NavigateToString(res.HtmlDescription);

                TorrentWrapper torrentWrapper = TorrentsManager.StartTorrent(res.MagnetURI, Constants.TorrentDownloadFolder);
                if (torrentWrapper != null)
                    SetupTorrentOperationsUI(torrentWrapper);
                else
                    MessageBox.Show("Impossibile avviare il torrent selezionato: errore durante l'aggiunta del torrent alla sessione", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeTorrentContent(string filePath)
        {
            CurrentContentType = ParseResult.ParseResultType.TorrentContent;
            SetEnabledControls(true);

            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            tbTorrentTitle.Text = fileName;
            browserTorrentDescription.NavigateToString("<html><body></body></html>");

            TorrentWrapper torrentWrapper = TorrentsManager.StartTorrentFromFile(filePath, Constants.TorrentDownloadFolder);
            SetupTorrentOperationsUI(torrentWrapper);

            tcMain.SelectedItem = tabContenuti;
        }

        private void SetupTorrentOperationsUI(TorrentWrapper torrentWrapper)
        {
            if (torrentWrapper != null)
            {
                TorrentOperations torrentOperations = new TorrentOperations(torrentWrapper, TaskbarItemInfo);
                TabItem ti = new TabItem();
                ti.Header = "Torrent attivo";
                ti.Content = torrentOperations;
                tabLinks.Items.Clear();
                tabLinks.Items.Add(ti);
                tabLinks.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Errore durante l'aggiunta del torrent.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearContent()
        {
            CurrentContent = null;
            treeEpisodes.ItemsSource = null;
            descriptionGrid.Visibility = Visibility.Hidden;
            lblSeasonCount.Text = lblEpisodeCount.Text = string.Empty;
            imgCover.Source = null;
            lblDescription.Text = string.Empty;
            browserMovieLinks.NavigateToString("&nbsp;");
            //browserMovieLinks.DocumentText = "&nbsp;";
            ClearTabLinks();
            SetEnabledControls();
        }

        private void ClearTabLinks()
        {
            var deftab = tabItemDefaultMessage;
            tabLinks.Items.Clear();
            tabLinks.Items.Add(deftab);
            tabLinks.SelectedItem = deftab;
        }

        #endregion

        #region UI manipulators (show/hide, enable/disable, zoom, cover image, etc.)

        private void ShowLoadingPanels()
        {
            lp1.Visibility = lp2.Visibility = System.Windows.Visibility.Visible;
            treeEpisodes.Visibility = descriptionGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void HideLoadingPanels()
        {
            lp1.Visibility = lp2.Visibility = System.Windows.Visibility.Hidden;
            treeEpisodes.Visibility = descriptionGrid.Visibility = System.Windows.Visibility.Visible;
        }

        private void SetEnabledControls(bool ignoreNull = false)
        {
            groupInfoPanel.Visibility = Visibility.Visible;
            groupLinkFilm.SetValue(Grid.ColumnSpanProperty, 1);

            if (CurrentContent == null && !ignoreNull)
            {
                groupLinksAndDeskContainer.Visibility = Visibility.Hidden;
                groupCurrentTorrent.Visibility = Visibility.Hidden;
                stackNoContentMessage.Visibility = Visibility.Visible;
            }
            else if (CurrentContentType == ParseResult.ParseResultType.NestedContent)
            {
                groupLinksAndDeskContainer.Visibility = Visibility.Visible;
                groupCurrentTorrent.Visibility = Visibility.Hidden;
                groupLinkFilm.Visibility = Visibility.Hidden;
                groupListaEpisodi.Visibility = Visibility.Visible;

                tbMovieTitle.Visibility = Visibility.Hidden;
                tbShowInfo.Visibility = Visibility.Visible;

                stackNoContentMessage.Visibility = Visibility.Hidden;
                groupLinksAndDeskContainer.Header = "Serie selezionata";

            }
            else if (CurrentContentType == ParseResult.ParseResultType.HtmlContent)
            {
                groupLinksAndDeskContainer.Visibility = Visibility.Visible;
                groupCurrentTorrent.Visibility = Visibility.Hidden;
                groupLinkFilm.Visibility = Visibility.Visible;
                groupListaEpisodi.Visibility = Visibility.Hidden;

                tbMovieTitle.Visibility = Visibility.Visible;
                tbShowInfo.Visibility = Visibility.Hidden;

                stackNoContentMessage.Visibility = Visibility.Hidden;
                groupLinksAndDeskContainer.Header = "Film selezionato";
            }
            else if (CurrentContentType == ParseResult.ParseResultType.TorrentContent)
            {
                groupCurrentTorrent.Visibility = Visibility.Visible;
                groupLinksAndDeskContainer.Visibility = Visibility.Hidden;

                stackNoContentMessage.Visibility = Visibility.Hidden;
            }

            SetBookmarkButtonsVisibility();

        }

        private void SetBookmarkButtonsVisibility()
        {
            if (CurrentContent == null)
                btnAddToBookmarks.Visibility = btnDeleteBookmark.Visibility = System.Windows.Visibility.Hidden;
            else if (Repo.IsBookmarkAlreadyPresent(CurrentContent))
            {
                btnAddToBookmarks.Visibility = System.Windows.Visibility.Hidden;
                btnDeleteBookmark.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnAddToBookmarks.Visibility = System.Windows.Visibility.Visible;
                btnDeleteBookmark.Visibility = System.Windows.Visibility.Hidden;
            }

        }


        private void SetCoverImageAsync(string imageURL)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        var webClient = new WebClient();
                        var url = new Uri(imageURL, UriKind.Absolute);
                        var buffer = webClient.DownloadData(url);
                        var bitmapImage = new BitmapImage();

                        using (var stream = new MemoryStream(buffer))
                        {
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = stream;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();
                        }

                        Dispatcher.BeginInvoke((Action)(() => imgCover.Source = bitmapImage));
                    }
                    catch (Exception)
                    {
                        return;
                    }

                });
            }
            catch (Exception)
            {
                return;
            }
        }


        private void SetZoomFromComboBox()
        {
            float scale = float.Parse(((ComboBoxItem)comboZoom.SelectedItem).Tag.ToString());
            ScaleTransform zt = new ScaleTransform(scale, scale);
            parentGrid.LayoutTransform = zt;
        }

        private void SetDefaultTabFromComboBox()
        {
            int index = int.Parse((comboDefaultTab.SelectedItem as ComboBoxItem).Tag.ToString());
            tcMain.SelectedIndex = index;
        }

        #endregion

        #region Tree and Browser

        private void treeEpisodes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var def = tabItemDefaultMessage;
            foreach (TabItem item in tabLinks.Items)
            {
                var content = item.Content;
                if (item != def)
                    item.Content = null;
            }
            tabLinks.Items.Clear();

            if (treeEpisodes.SelectedItem != null && (treeEpisodes.SelectedItem is ContentNestBlock))
            {
                ContentNestBlock ep = (ContentNestBlock)treeEpisodes.SelectedItem;
                if (ep.Links.Count == 0)
                    return;

                tabItemDefaultMessage.Visibility = System.Windows.Visibility.Collapsed;
                foreach (var item in ep.Links)
                {
                    LinkOperations lo = new LinkOperations(item, CurrentContent.Url);
                    TabItem ti = new TabItem();
                    ti.Header = DomainExtractor.GetDomainFromUrl(item.Url);
                    ti.Content = lo;
                    tabLinks.Items.Add(ti);
                }
            }
            else
            {
                def.Visibility = System.Windows.Visibility.Visible;
                tabLinks.Items.Add(def);
            }
            tabLinks.SelectedItem = tabLinks.Items[0];
        }

        private void browserMovieLinks_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                e.Cancel = true;
                tabLinks.Items.Clear();

                string url = e.Uri.ToString();
                Link item = new Link("", url);
                LinkOperations lo = new LinkOperations(item, CurrentContent.Url);
                TabItem ti = new TabItem();
                ti.Header = DomainExtractor.GetDomainFromUrl(item.Url);
                ti.Content = lo;
                tabLinks.Items.Add(ti);

                tabLinks.SelectedItem = tabLinks.Items[0];
            }

        }

        #endregion

        #region Options and About screen

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AboutWindow();
            dlg.Owner = this;
            dlg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dlg.WindowStyle = System.Windows.WindowStyle.ToolWindow;
            dlg.ShowDialog();
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Visibility = System.Windows.Visibility.Hidden;
            gbSettings.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnSetPotplayerPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "PotPlayerMini.exe|PotPlayerMini.exe";
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true && File.Exists(dlg.FileName))
            {
                Repo.Settings.PathToPotplayerExe = dlg.FileName;
            }
        }

        private void btnSetVLCPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "vlc.exe|vlc.exe";
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true && File.Exists(dlg.FileName))
            {
                Repo.Settings.PathToVLCExe = dlg.FileName;
            }
        }

        private void btnSetIDMPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "IDMan.exe|IDMan.exe";
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true && File.Exists(dlg.FileName))
            {
                Repo.Settings.PathToPotplayerExe = dlg.FileName;
            }
        }

        private void saveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Repo.Settings.ZoomIndex = comboZoom.Items.IndexOf(comboZoom.SelectedItem);
                Repo.Settings.DefaultTab = comboDefaultTab.Items.IndexOf(comboDefaultTab.SelectedItem);

                Repo.Settings.CheckForUpdates = chkCheckForUpdates.IsChecked.Value;
                Repo.Settings.MinimizeToTray = chkMinimizeToTray.IsChecked.Value;
                Repo.Settings.OnLinkSentDownloadImmediately = rbOnLinkDownload.IsChecked.HasValue ? rbOnLinkDownload.IsChecked.Value : false;

                //enable/disable google suggestions app-wide
                Repo.Settings.UseGoogleSuggestions = chkUseGoogleSuggestions.IsChecked.Value;
                if (Repo.Settings.UseGoogleSuggestions)
                {
                    actb.Provider = SuggestionsProvider;
                }
                else
                {
                    actb.Provider = null;
                }

                //enable/disable Chrome extension service
                Repo.Settings.EnableExtensionService = chkEnableExtensionService.IsChecked.Value;
                if (Repo.Settings.EnableExtensionService)
                    chkEnableExtensionService.IsChecked = ServicesManager.StartListening();
                else
                    ServicesManager.StopListening();

                //History settings
                Repo.Settings.TrackHistory = chkTrackHistory.IsChecked.Value;
                if (!Repo.Settings.TrackHistory)
                {
                    HistoryManager.ClearLocalHistory();
                }
                Repo.Settings.UseSharedHistory = chkSupportSharedHistory.IsChecked.Value;
                if (!Repo.Settings.UseSharedHistory)
                    HistoryManager.SharedModeActive = false;
                Repo.Settings.EDFAccount_Password = txt_EDFpass.Password;
                Repo.Settings.EDFAccount_Username = txt_EDFuser.Text;
                setupSharedHistoryUI(); //updates UI elements related to the shared UI
                ReloadHistory();

                //Torrent settings
                Repo.Settings.TorrentDownloadPath = txtTorrentDownloadPath.Text.Trim();
                Repo.Settings.TorrentDownloadPort = (int)txtPortNumber.Value;

                //All done, save settings
                Repo.SaveSettings();

                //force refreshes in case loaded content needs to update
                if (CurrentContentType == ParseResult.ParseResultType.NestedContent)
                    treeEpisodes_SelectedItemChanged(treeEpisodes, null);

                //hide settings, show main UI
                mainGrid.Visibility = System.Windows.Visibility.Visible;
                gbSettings.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Eccezione durante il salvataggio delle impostazioni", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

        }

        private void ClearHistory()
        {
            if (CurrentContent != null)
            {
                if (CurrentContentType == ParseResult.ParseResultType.NestedContent)
                {
                    List<ContentNestBlock> cnbs = new List<ContentNestBlock>();
                    foreach (var item in treeEpisodes.Items)
                    {
                        if (item is NestBlock)
                            cnbs.AddRange((item as NestBlock).GetContentBlocks());
                        else if (item is ContentNestBlock)
                            cnbs.Add(item as ContentNestBlock);
                    }

                    foreach (var item in cnbs)
                    {
                        item.LastAccess = null;
                    }
                }
            }
        }

        private void comboZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetZoomFromComboBox();
        }

        private void btnInstallExtension_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ExtensionInstallationWindow();
            dlg.Owner = this;
            dlg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dlg.WindowStyle = System.Windows.WindowStyle.ToolWindow;
            dlg.ShowDialog();
        }

        #endregion

        #region Uninstall and factory reset

        private void btnClean_Click(object sender, RoutedEventArgs e)
        {
            var resp = MessageBox.Show("EraDeiFessi verrà ripristinato alle impostazioni di fabbrica. Ogni file temporaneo relativo a EraDeiFessi verrà rimosso e il programma sarà come appena installato.\n\nQuesta operazione non è reversibile\n\nSi desidera proseguire?", "Richiesta di conferma", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resp == MessageBoxResult.Yes)
            {
                string bat = System.IO.Path.GetTempPath() + "\\EDFuninstall.bat";
                if (!IOHelper.ExtractResource("uninstall.bat", bat))
                {
                    MessageBox.Show("Errore: script di ripristino non trovato. Impossibile procedere", "Errore fatale", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var psi = new ProcessStartInfo();
                psi.CreateNoWindow = false;
                psi.FileName = @"cmd.exe";
                psi.Arguments = "/C " + bat + " CLEAN \"" + Constants.EDFAppdataFolder + "\"";

                try
                {
                    var process = new Process();
                    process.StartInfo = psi;
                    process.Start();
                }
                catch (Exception)
                {
                    MessageBox.Show("Errore durante l'esecuzione dello script di pulizia.", "Errore non gestito", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                App.Current.Shutdown();
            }
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            var resp = MessageBox.Show("ATTENZIONE: EraDeiFessi e tutte le sue impostazioni verranno rimossi dal sistema.\n\nQuesta operazione non è reversibile\n\nNOTA: verranno richiesti i permessi di amministrazione per eseguire la disinstallazione.\n\nSi desidera proseguire?", "Richiesta di conferma", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resp == MessageBoxResult.Yes)
            {
                string bat = System.IO.Path.GetTempPath() + "\\EDFuninstall.bat";
                if (!IOHelper.ExtractResource("uninstall.bat", bat))
                {
                    MessageBox.Show("Errore: script di disinstallazione non trovato. Impossibile procedere", "Errore fatale", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var psi = new ProcessStartInfo();
                psi.CreateNoWindow = false;
                psi.FileName = @"cmd.exe";
                psi.Arguments = "/C " + bat + " UNINSTALL \"" + Constants.ProgramLocation + "\" \"" + Constants.EDFAppdataFolder + "\"";
                psi.Verb = "runas";

                try
                {
                    var process = new Process();
                    process.StartInfo = psi;
                    process.Start();
                }
                catch (Exception)
                {
                    MessageBox.Show("Non hai fornito i permessi di amministrazione, pertanto non sono in grado di procedere alla disinstallazione", "Permessi negati", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                App.Current.Shutdown();
            }

        }

        #endregion

        #region Search

        private void goToSearchSection_Click(object sender, RoutedEventArgs e)
        {
            tcMain.SelectedItem = tabCerca;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(actb.SelectedItem as string))
                Search(actb.SelectedItem as string);
            else if (!string.IsNullOrWhiteSpace(actb.Text))
                Search(actb.Text);
        }

        private void Search(string query)
        {
            tcSearch.Items.Clear();

            foreach (var plugin in PluginsRepo.Plugins.Values)
            {
                IEDFSearch searchplugin = plugin as IEDFSearch;
                if (plugin is IEDFSearch && (chkSearchAll.IsChecked.Value || EnabledSearchPlugins.Contains(searchplugin)))
                {
                    PerformSearch(searchplugin, query);
                }
            }

            if (tcSearch.Items.Count > 0)
                btnClearSearch.Visibility = System.Windows.Visibility.Visible;
        }

        private async void PerformSearch(IEDFSearch searchplugin, string searchterm)
        {
            BookmarkManager bmg = new BookmarkManager(new List<Bookmark>(), false) { Plugin = searchplugin };
            bmg.ShowLoadingPanel();

            //TabItem ti = new TabItem() { Content = bmg, Header = ((IEDFPlugin)searchplugin).pluginName };
            SearchTabHeader sth = new SearchTabHeader(((IEDFPlugin)searchplugin).pluginName);
            TabItem ti = new TabItem() { Content = bmg, Header = sth };
            tcSearch.Items.Add(ti);

            if (tcSearch.SelectedIndex == -1)
                tcSearch.SelectedIndex = 0;

            var searchres = await ParsingHelpers.PerformSearchAsync(searchplugin, searchterm);
            string nextpage = null;
            IEnumerable<Bookmark> res = null;

            if (searchres != null)
            {
                res = searchres.Result;
                nextpage = searchres.NextPageUrl;
            }

            sth.ShowSearchImage = false;
            if (res == null || res.Count() == 0)
            {
                sth.SetCounter(0);
                bmg.ShowBackgroundMessage("Nessun risultato trovato");
            }
            else
            {
                sth.SetCounter(res.Count());
                foreach (var item in res)
                    bmg.Bookmarks.Add(item);
            }

            bmg.NextResultPage = nextpage;
            bmg.RecheckNextPageButtonVisibility();

            bmg.HideLoadingPanel();
        }

        private async void GetMoreResults(object sender, BookmarkManager.GetMoreResultsEventArgs e)
        {
            BookmarkManager bmg = e.BookmarkManager;
            bmg.ShowLoadingMorePanel();

            SearchTabHeader sth = (bmg.Parent as TabItem).Header as SearchTabHeader;

            var searchres = await ParsingHelpers.GetResultPageAsync(bmg.Plugin, bmg.NextResultPage);
            if (searchres != null)
            {
                var res = searchres.Result;
                if (res != null)
                {
                    foreach (var item in searchres.Result)
                    {
                        bmg.Bookmarks.Add(item);
                    }
                }
                bmg.NextResultPage = searchres.NextPageUrl;
                sth.SetCounter(bmg.Bookmarks.Count);
            }

            bmg.HideLoadingMorePanel();
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            actb.Text = "";
            tcSearch.Items.Clear();
            btnClearSearch.Visibility = System.Windows.Visibility.Hidden;
        }

        private void chkSearchPlugin_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            IEDFSearch plugin = chk.DataContext as IEDFSearch;

            if (chk.IsChecked.Value && !EnabledSearchPlugins.Contains(plugin))
                EnabledSearchPlugins.Add(plugin);
            else if (!chk.IsChecked.Value && EnabledSearchPlugins.Contains(plugin))
                EnabledSearchPlugins.Remove(plugin);

            var toSearch = (from s in EnabledSearchPlugins select (s as IEDFPlugin).pluginID).ToArray();
            Repo.Settings.SelectedSearchPlugins = toSearch;
        }

        private void chkSearchThis_Loaded(object sender, RoutedEventArgs e)
        {
            var item = (sender as CheckBox).DataContext;
            if (Repo.Settings.SelectedSearchPlugins != null && Repo.Settings.SelectedSearchPlugins.Contains((item as IEDFPlugin).pluginID))
            {
                (sender as CheckBox).IsChecked = true;
                EnabledSearchPlugins.Add(item as IEDFSearch);
            }
        }

        private void chkSearchAll_Click(object sender, RoutedEventArgs e)
        {
            Repo.Settings.SearchAllPlugins = (sender as CheckBox).IsChecked.Value;
        }

        private void actb_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Return) && actb.SelectedItem != null)
            {
                string q = actb.SelectedItem as string;
                if (!string.IsNullOrWhiteSpace(q))
                    Search(q);
            }
        }

        private void actb_TextChanged_2(string newText)
        {
            btnSearch.IsEnabled = !string.IsNullOrWhiteSpace(newText);
        }

        #endregion

        #region Extension and EDF service interaction

        private void UnblockRequestIncoming(object sender, EDFServices.RequestReceivedEventArgs e)
        {
            if (Repo.Settings.OnLinkSentDownloadImmediately)
            {
                myLinkUnblocker.UnblockLink(e.Link, true);
            }
            else
            {
                RestoreAndShow();

                tcMain.SelectedItem = tabUnblocker;
                myLinkUnblocker.UnblockLink(e.Link, false);
            }

        }

        #endregion

        #region Tray Icon and Single Instance management

        private void notifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            RestoreAndShow();
        }

        public void RestoreAndShow()
        {
            notifyIcon.Visibility = System.Windows.Visibility.Collapsed;
            this.Show();
            this.Activate();
            if (this.WindowState == System.Windows.WindowState.Minimized)
                this.WindowState = System.Windows.WindowState.Normal;
        }

        private void w1_StateChanged(object sender, EventArgs e)
        {
            if (Repo.Settings.MinimizeToTray && this.WindowState == System.Windows.WindowState.Minimized)
            {
                notifyIcon.Visibility = System.Windows.Visibility.Visible;
                this.Hide();
            }
        }

        #endregion

        #region Plugin configuration

        private void btnPluginConfig_Loaded(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            IEDFPluginOptions p = b.DataContext as IEDFPluginOptions;
            if (p != null)
                b.Visibility = System.Windows.Visibility.Visible;
            else
                b.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnPluginConfig_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            IEDFPluginOptions p = b.DataContext as IEDFPluginOptions;
            if (p != null)
            {
                Popup pop = new Popup();

                Grid firstGrid = new Grid();
                firstGrid.Background = this.Background;

                Border br = new Border();
                br.BorderThickness = new Thickness(1);
                br.CornerRadius = new CornerRadius(8);
                br.BorderBrush = Brushes.DarkGray;
                firstGrid.Children.Add(br);

                Grid gr = new Grid();
                gr.Background = this.Background;
                gr.Margin = new Thickness(4);

                br.Child = gr;

                GroupBox g = new GroupBox();

                g.Header = "Opzioni di ricerca: " + (p as IEDFPlugin).pluginName;
                //g.Content = p.OptionsPanel;
                PropGrid pg = new PropGrid();
                pg.DataSource = p.Options;
                g.Content = pg;

                gr.Children.Add(g);
                pop.Child = firstGrid;
                pop.PlacementTarget = b;
                pop.StaysOpen = false;
                pop.AllowsTransparency = true;
                pop.IsOpen = true;

            }
        }

        #endregion

        #region Drag & Drop

        private void fileDropped(object sender, DragEventArgs e)
        {
            string droppedFilename = (e.Data.GetData(DataFormats.FileDrop, true) as string[])[0];

            ChangeTorrentContent(droppedFilename);
        }

        private void mainGrid_DragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                if (filenames.Length == 1 && System.IO.Path.GetExtension(filenames[0]).ToUpper() == ".TORRENT")
                    dropEnabled = true;
            }

            if (!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        #endregion

        #region Torrenting

        private void btnEditTorrentDownloadPath_Click(object sender, RoutedEventArgs e)
        {

        }

        private void browserTorrentDescription_LoadCompleted(object sender, NavigationEventArgs e)
        {

        }

        private bool AskStopTorrentIfRunning(bool closing) //returns true if loading should proceed
        {
            try
            {
                if (TorrentsManager.CurrentTorrent != null)
                {
                    string verb = closing ? "uscire" : "caricare il prosismo contenuto";
                    string message = "";
                    MessageBoxButton button;
                    if (TorrentsManager.CurrentTorrent.State == Ragnar.TorrentState.Finished || TorrentsManager.CurrentTorrent.State == Ragnar.TorrentState.Seeding)
                    {
                        message = string.Format("Il torrent attualmente attivo è stato scaricato completamente. Vuoi cancellare i file scaricati prima di {0}?", verb);
                        button = MessageBoxButton.YesNo;
                    }
                    else
                    {
                        message = string.Format("ATTENZIONE: il torrent attualmente attivo è ancora in fase di download! Se procedi il download verrà interrotto.\n\nVuoi cancellare i file parzialmente scaricati prima di {0}?\n\n(premi 'Annulla' se non vuoi {0} e desideri invece continuare a scaricare/streamare il torrent corrente", verb);
                        button = MessageBoxButton.YesNoCancel;
                    }

                    var res = MessageBox.Show(message, "Torrent attivo", button, MessageBoxImage.Question);

                    if (res != MessageBoxResult.Cancel)
                    {
                        try
                        {
                            TorrentOperations to = tabLinks.SelectedContent as TorrentOperations;
                            to.Stop();
                        }
                        catch { }

                        bool removeData = res == MessageBoxResult.Yes;
                        TorrentsManager.RemoveTorrent(removeData);
                        TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        TaskbarItemInfo.ProgressValue = 0;
                    }

                    return res != MessageBoxResult.Cancel;
                }
            }
            catch (Exception)
            {
            }
            return true;
        }

        #endregion

        #region Real-Debrid

        private void AuthorizeRealDebrid(bool forceReAuthorization)
        {
            if (!Repo.RDAgent.Authorized || forceReAuthorization)
            {
                RealDebridAuthenticationWizard wiz = new RealDebridAuthenticationWizard(Repo.RDAgent);
                wiz.ShowDialog();
            }

            if (Repo.RDAgent.Authorized & !Repo.RDAgent.TokenInDate)
                Repo.RDAgent.RefreshAuthorizationToken();

            RecheckRD();
        }

        private void DeauthorizeRealDebrid()
        {
            Repo.LogoutRD();
            Repo.RDAgent.TokenRefreshed += (trsender, tre) => { RecheckRD(); };
            RecheckRD();
        }

        private void RecheckRD()
        {
            Dispatcher.Invoke(() =>
            {
                btnAutorizzaAccount.IsEnabled = !(Repo.RDAgent.Authorized);
                btnDeautorizzaAccount.IsEnabled = (Repo.RDAgent.Authorized);

                txtStatoRD.Text = Repo.RDAgent.Authorized ? (Repo.RDAgent.TokenInDate ? "Autorizzato e connesso!" : "Autorizzato, ma token scaduto! Verrà rigenerato automaticamente alla prossima richiesta.") : "Non connesso / autorizzato. Usa il pulsante sottostante per autorizzare il tuo account.";
                txtStatoRD.Foreground = Repo.RDAgent.Authorized ? (Repo.RDAgent.TokenInDate ? Brushes.ForestGreen : Brushes.Orange) : Brushes.Red;

                txtTokenRD.Text = Repo.RDAgent.Authorized ? Repo.RDAgent.Token.access_token : "N/A";
                txtScadenzaTokenRD.Text = Repo.RDAgent.Authorized ? Repo.RDAgent.Token.expirationDate.ToString("dd/MM/yyyy alle hh:mm:ss") : "N/A";

                foreach (TabItem item in tabLinks.Items)
                {
                    LinkOperations lo = item.Content as LinkOperations;
                    if (lo != null)
                        lo.RecheckAll();
                }
            });
        }

        private void btnAutorizzaAccount_Click(object sender, RoutedEventArgs e)
        {
            AuthorizeRealDebrid(true);
        }

        private void btnDeautorizzaAccount_Click(object sender, RoutedEventArgs e)
        {
            DeauthorizeRealDebrid();
        }


        #endregion


    }
}
