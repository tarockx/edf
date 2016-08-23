using EraDeiFessi.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using RealDebrid4DotNet.RestModel;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for LinkUnblocker.xaml
    /// </summary>
    public partial class LinkUnblocker : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<LinkUnrestrictRequestResponse> UnblockedLinks { get; set; }
        private IEnumerable<LinkUnrestrictRequestResponse> SelectedUnblockedLinksWithoutErrors { get { return (from LinkUnrestrictRequestResponse i in listUnblocked.SelectedItems where !i.has_error select i); } }

        public bool Working { get; set; }

        public bool CanUnblock
        {
            get { return !string.IsNullOrWhiteSpace(txtLinks.Text) && (Repo.RDAgent.Authorized) && !Working; }
        }
        public bool CanStream
        {
            get { return SingleLinkSelected && File.Exists(Repo.Settings.PathToPotplayerExe); }
        }
        public bool CanDownload
        {
            get { return LinksSelected && File.Exists(Repo.Settings.PathToIDMExe); }
        }

        private int _UnblockProgress = 0;
        public int UnblockProgress
        {
            get { return _UnblockProgress; }
            set
            {
                _UnblockProgress = value;
                OnPropertyChanged("UnblockProgress");
            }
        }

        public bool LinksSelected { get { return listUnblocked.SelectedItems.Count > 0 && SelectedUnblockedLinksWithoutErrors.Count() > 0; } }
        public bool SingleLinkSelected { get { return listUnblocked.SelectedItems.Count == 1 && !(listUnblocked.SelectedItem as LinkUnrestrictRequestResponse).has_error; } }



        public LinkUnblocker()
        {
            UnblockedLinks = new ObservableCollection<LinkUnrestrictRequestResponse>();
            Working = false;

            InitializeComponent();

            CheckIfCanUnblock();
        }

        private void CheckIfCanUnblock()
        {
            OnPropertyChanged("CanUnblock");
            if (!(Repo.RDAgent.Authorized))
            {
                buttonUnblock.ToolTip = "Devi essere loggato su RealDebrid per poter sbloccare i link. Puoi loggarti dal menu opzioni";
            }
            else if (string.IsNullOrWhiteSpace(txtLinks.Text))
            {
                buttonUnblock.ToolTip = "Inserisci almeno un link da sbloccare nel riquadro sovrastante";
            }
            else
            {
                buttonUnblock.ToolTip = null;
            }
        }

        private void CheckLinksSelected()
        {
            OnPropertyChanged("LinksSelected");
            OnPropertyChanged("SingleLinkSelected");
            OnPropertyChanged("CanStream");
            OnPropertyChanged("CanDownload");
        }

        private void btnUnblock_Click(object sender, RoutedEventArgs e)
        {
            RunUnblock();
        }

        private async void RunUnblock(bool runAsync = true)
        {
            Working = true;
            CheckIfCanUnblock();

            UnblockedLinks.Clear();
            CheckLinksSelected();

            gridUUnblocking.Visibility = System.Windows.Visibility.Visible;
            var links = from l in txtLinks.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries) where !string.IsNullOrWhiteSpace(l) select l.Trim();
            UnblockProgress = 0;
            pbUnblock.Minimum = 0;
            pbUnblock.Maximum = links.Count();
            tbTotal.Text = links.Count().ToString();

            if (runAsync)
                await UnblockLinksAsync(links);
            else
                UnblockLinks(links);

            if (listUnblocked.Items.Count == 1)
            {
                listUnblocked.SelectedIndex = 0;
            }

            gridUUnblocking.Visibility = System.Windows.Visibility.Hidden;
            Working = false;
            CheckIfCanUnblock();
        }

        private Task UnblockLinksAsync(IEnumerable<string> links)
        {
            var task = Task.Factory.StartNew(() => this.UnblockLinks(links));
            return task;
        }

        private void UnblockLinks(IEnumerable<string> links)
        {

            var agent = Repo.RDAgent;

            List<LinkUnrestrictRequestResponse> unblockedLinks = new List<LinkUnrestrictRequestResponse>();

            foreach (string link in links)
            {
                var resp = agent.UnrestrictLink(link);

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<LinkUnrestrictRequestResponse>(ReportProgress), resp);
            }
        }

        private void ReportProgress(LinkUnrestrictRequestResponse resp)
        {
            UnblockedLinks.Add(resp);
            UnblockProgress++;
        }

        private void btnRDCopy_Click(object sender, RoutedEventArgs e)
        {
            string result = string.Empty;
            foreach (LinkUnrestrictRequestResponse link in SelectedUnblockedLinksWithoutErrors)
            {
                result = result + link.download + "\n";
            }
            Clipboard.SetText(result.Trim());
        }

        private void btnRDDownload_Click(object sender, RoutedEventArgs e)
        {
            var links = SelectedUnblockedLinksWithoutErrors;

            if (System.IO.File.Exists(Repo.Settings.PathToIDMExe))
            {
                if (links.Count() == 1)
                {
                    string path = Repo.Settings.PathToIDMExe;
                    string args = string.Format("/d \"{0}\"", links.ElementAt(0).download);
                    Process.Start(path, args);
                }
                else
                {
                    foreach (var item in links)
                    {
                        string path = Repo.Settings.PathToIDMExe;
                        string args = string.Format("/d \"{0}\" /a", item.download);
                        var p = Process.Start(path, args);
                        p.WaitForExit();
                    }

                    Process.Start(Repo.Settings.PathToIDMExe);
                }
            }
        }

        private void btnRDSelectAll_Click(object sender, RoutedEventArgs e)
        {
            listUnblocked.SelectAll();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void txtLinks_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfCanUnblock();
        }

        private void listUnblocked_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckLinksSelected();
        }

        private void btnRDStream_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Repo.Settings.PathToPotplayerExe))
            {
                string path = Repo.Settings.PathToPotplayerExe;
                Process.Start(path, (listUnblocked.SelectedItem as LinkUnrestrictRequestResponse).download);
            }
        }

        public void UnblockLink(string url, bool downloadImmediately)
        {
            txtLinks.Text = url.Trim();
            if (CanUnblock)
                RunUnblock(!downloadImmediately);
            if (downloadImmediately)
            {
                var links = SelectedUnblockedLinksWithoutErrors;

                if (File.Exists(Repo.Settings.PathToIDMExe) && links.Count() > 0)
                {
                    string path = Repo.Settings.PathToIDMExe;
                    string args = string.Format("/d \"{0}\"", links.ElementAt(0).download);
                    Process.Start(path, args);
                }
            }

        }

        private void LoadingPanel_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as LoadingPanel).Size = 16;
        }


    }
}
