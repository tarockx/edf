using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EraDeiFessi.Repository;
using libEraDeiFessi;
using EraDeiFessi.Helpers;
using EraDeiFessi.Parser;
using RealDebrid4DotNet.RestModel;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for LinkOperations.xaml
    /// </summary>
    public partial class LinkOperations : UserControl
    {

        LinkUnrestrictRequestResponse RDResponse = null;
        Link EpisodeLink;
        string Referer;

        public LinkOperations(Link episode, string referer)
        {
            EpisodeLink = episode;
            Referer = referer;

            InitializeComponent();

            ScanLink();
            RecheckAll();
        }

        private void ScanLink()
        {
            stackLinkProtected.Visibility = stackLinkSupported.Visibility = stackLinkUnsupported.Visibility = System.Windows.Visibility.Collapsed;

            bool supported = false;
            string url = EpisodeLink.Url.ToLower();

            supported = Repo.RDAgent.LinkSupported(url);

            if(supported){
                stackLinkSupported.Visibility = System.Windows.Visibility.Visible;
                return;
            }

            if(WebBypasser.IsLinkSupported(url)){
                stackLinkProtected.Visibility = System.Windows.Visibility.Visible;
                return;
            }

            stackLinkUnsupported.Visibility = System.Windows.Visibility.Visible;
        }

        public void RecheckAll(){
            CheckIfLoggedIntoRD();
            CheckIfPotPlayerAvailable();
            CheckIfVLCAvailable();
            CheckIfIDMAvailable();
        }

        public void CheckIfLoggedIntoRD()
        {
            btnUnrestrict.IsEnabled = (Repo.RDAgent.Authorized);
        }
        public void CheckIfPotPlayerAvailable()
        {
            btnRDStreamPot.IsEnabled = System.IO.File.Exists(Repo.Settings.PathToPotplayerExe);
        }
        public void CheckIfVLCAvailable()
        {
            btnRDStreamVLC.IsEnabled = System.IO.File.Exists(Repo.Settings.PathToVLCExe);
        }
        public void CheckIfIDMAvailable()
        {
            btnRDDownload.IsEnabled = System.IO.File.Exists(Repo.Settings.PathToIDMExe);
        }


        private void btnUnrestrict_Click(object sender, RoutedEventArgs e)
        {
            if (Parser.WebBypasser.IsLinkSupported(EpisodeLink.Url))
                BypassAndUnrestrict(EpisodeLink.Url);
            else
                Unrestrict(EpisodeLink.Url);
        }

        async private void BypassAndUnrestrict(string link)
        {
            ShowLoadingPanel("Sto tentando di bypassare il link protetto... attendere...");
            string resp = await Parser.WebBypasser.BypassAsync(link, Referer);
            HideLoadingPanel();

            if (string.IsNullOrWhiteSpace(resp))
            {
                lblRDLinkInfo.Visibility = System.Windows.Visibility.Collapsed;
                panelError.Visibility = System.Windows.Visibility.Visible;
                panelRDButtons.Visibility = System.Windows.Visibility.Hidden;
                lblError.Text = "Non sono riuscito a superare il link protetto. Si consiglia di aprire il link a mano nel browser.";
            }
            else
                Unrestrict(resp);
        }

        async private void Unrestrict(string link)
        {
            ShowLoadingPanel("");
            RDResponse = await Repo.RDAgent.UnrestrictLinkAsync(link);
            HideLoadingPanel();

            if (!RDResponse.has_error)
            {
                lblRDLinkInfo.Visibility = System.Windows.Visibility.Visible;
                panelError.Visibility = System.Windows.Visibility.Hidden;
                panelRDButtons.Visibility = System.Windows.Visibility.Visible;
                lblFilename.Text = RDResponse.filename;
                lblFilesize.Text = RDResponse.FormattedFilesize;
                lblError.Text = "";

                if (Repo.Settings.TrackHistory)
                    HistoryManager.SetHistory(link, HistoryAction.Unblocked); //set "last unlocked" history point
            }
            else
            {
                lblRDLinkInfo.Visibility = System.Windows.Visibility.Collapsed;
                panelError.Visibility = System.Windows.Visibility.Visible;
                panelRDButtons.Visibility = System.Windows.Visibility.Hidden;
                lblError.Text = RDResponse.error_message;
            }
            panelRDButtons.IsEnabled = (RDResponse != null && !RDResponse.has_error);
            
            //if (!string.IsNullOrEmpty(RDResponse.StackTrace) && App.SHOW_DEBUG_MESSAGES)
            //    MessageBox.Show("Eccezione non gestita: \n\n" + RDResponse.ErrorMessage + "\n\n" + RDResponse.StackTrace, "Eccezione non gestita", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowLoadingPanel(string message)
        {
            panelError.Visibility = panelRDButtons.Visibility = lblRDLinkInfo.Visibility = System.Windows.Visibility.Hidden;
            stackLoading.Visibility = System.Windows.Visibility.Visible;
            txtLoadingMessage.Text = message;
            btnUnrestrict.IsEnabled = false;
        }

        private void HideLoadingPanel()
        {
            stackLoading.Visibility = System.Windows.Visibility.Hidden;
            txtLoadingMessage.Text = string.Empty;
            btnUnrestrict.IsEnabled = true;
        }

        private void btnRDStreamPot_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Repo.Settings.PathToPotplayerExe))
            {
                string path = Repo.Settings.PathToPotplayerExe;
                Process.Start(path, RDResponse.download);
            }
        }

        private void btnRDStreamVLC_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Repo.Settings.PathToVLCExe))
            {
                string path = Repo.Settings.PathToVLCExe;
                Process.Start(path, "-vvv \"" + RDResponse.download + "\"");
            }
        }

        private void btnRDDownload_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Repo.Settings.PathToIDMExe))
            {
                string path = Repo.Settings.PathToIDMExe;
                string args = string.Format("/d \"{0}\"", RDResponse.download);
                Process.Start(path, args);
            }
        }

        private void btnRDCopy_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(RDResponse.download);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            } 
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(EpisodeLink.Url);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            } 
        }

        private void btnOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (Repo.Settings.TrackHistory)
                HistoryManager.SetHistory(EpisodeLink.Url, HistoryAction.OpenedInBrowser); //set "last unlocked" history point
            Process.Start(EpisodeLink.Url);
        }

    }
}
