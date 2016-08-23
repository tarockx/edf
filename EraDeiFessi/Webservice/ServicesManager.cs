using EraDeiFessi.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EraDeiFessi.Webservice
{
    public static class ServicesManager
    {
        private static WebServiceHost ServiceHost { get; set; }

        public static bool StartListening()
        {
            if (ServiceHost != null)
                StopListening();

            try
            {
                ServiceHost = new WebServiceHost(typeof(EDFServices));
                try
                {
                    ServiceEndpoint ep = ServiceHost.AddServiceEndpoint(typeof(IEDFServices), new WebHttpBinding(), "http://localhost:60420/EDFServices");
                }
                catch (Exception)
                {
                    
                }
                
                ServiceHost.Open();
                return true;
            }
            catch (Exception)
            {

                bool? res = RegisterService();

                if (res.HasValue && res.Value)
                {
                    StartListening();
                    return true;
                }
                else
                {
                    if (!res.HasValue)
                        MessageBox.Show("Tentativo fallito: non hai fornito i permessi di amministrazione, pertanto non sono in grado di attivare il supporto all'estensione. Il supporto all'estensione verrà disabilitato. Ricorda che puoi riattivarlo in qualsiasi momento dal menu impostazioni", "Permessi negati", MessageBoxButton.OK, MessageBoxImage.Error);
                    Repository.Repo.Settings.EnableExtensionService = false;
                    return false;
                }
            }
        }

        public static void StopListening()
        {
            if(ServiceHost != null && ServiceHost.State == CommunicationState.Opened)
                ServiceHost.Close();
        }

        public static bool? RegisterService()
        {
            EnableServiceDialog dlg = new EnableServiceDialog();
            if(App.Current.MainWindow.IsLoaded)
                dlg.Owner = App.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? res = dlg.ShowDialog();

            if (res.HasValue && res.Value == true)
            {
                var psi = new ProcessStartInfo();
                psi.FileName = @"cmd.exe";
                psi.Arguments = "/C " + "netsh http delete urlacl url=http://+:60420/EDFServices & netsh http add urlacl url=http://+:60420/EDFServices user=Everyone";
                psi.Verb = "runas";

                try
                {
                    var process = new Process();
                    process.StartInfo = psi;
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception)
                {
                    return null;
                }
                

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
