using RealDebrid4DotNet;
using RealDebrid4DotNet.RestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for RealDebridAuthenticationWizard.xaml
    /// </summary>
    public partial class RealDebridAuthenticationWizard : Window
    {
        private RDAgent rd_agent;
        public bool Success { get; set; } = false;

        public RealDebridAuthenticationWizard(RDAgent agent)
        {
            rd_agent = agent;
            rd_agent.AuthenticationSuccessful += Rd_agent_AuthenticationSuccessful;
            rd_agent.AuthenticationFailed += Rd_agent_AuthenticationFailed;
            InitializeComponent();
        }

        private void Rd_agent_AuthenticationFailed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Success = false;
                MessageBox.Show("Errore durante la procedura di autorizzazione.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            });


        }

        private void Rd_agent_AuthenticationSuccessful(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Success = false;
                MessageBox.Show("Account autorizzato correttamente!", "Autorizzato", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            });
        }

        private void btnCancelAuthentication_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void aw1_Loaded(object sender, RoutedEventArgs e)
        {
            ObtainAuthenticationCodes();
        }

        private async void ObtainAuthenticationCodes()
        {
            try
            {
                var authenticationRequestData = await rd_agent.GetAuthorizationCodeAsync();

                panelObtainingCode.Visibility = Visibility.Collapsed;
                txtCode.Text = authenticationRequestData.user_code;
                txtCode.Visibility = Visibility.Visible;

                rd_agent.StartCheckingForAuthorization(authenticationRequestData);
                browserAuthentication.Navigate(authenticationRequestData.verification_url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore: impossibile recuperare codice di autorizzazione da Real-Debrid.\n\nEccezione: " + ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

        }

        private void aw1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                rd_agent.AuthenticationFailed -= Rd_agent_AuthenticationFailed;
                rd_agent.AuthenticationSuccessful -= Rd_agent_AuthenticationSuccessful;
                rd_agent.StopCheckingForAuthorization();
            }
            catch { }
        }
    }
}
