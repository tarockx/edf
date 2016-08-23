using Foundation;
using System;
using System.CodeDom.Compiler;
using ToastIOS;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_RDAuthenticator : UIViewController
	{
		public ViewController_RDAuthenticator (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            stackLoadingCode.Hidden = false;
            stackAuthorizationCode.Hidden = true;

            btnCopyAuthorizationCode.TouchUpInside += BtnCopyAuthorizationCode_TouchUpInside;

            GetAuthorizationCode();
        }

        private void BtnCopyAuthorizationCode_TouchUpInside(object sender, EventArgs e)
        {
            UIPasteboard.General.String = lblAuthorizationCode.Text;
            Toast.MakeText("Codice di autorizzazione copiato negli appunti").Show(ToastType.Info);
        }

        private async void GetAuthorizationCode()
        {
            try
            {
                Repo.RDHelper.RDAgent.AuthenticationSuccessful += RDAgent_AuthenticationSuccessful;
                Repo.RDHelper.RDAgent.AuthenticationFailed += RDAgent_AuthenticationFailed;

                var authenticationRequestData = await Repo.RDHelper.RDAgent.GetAuthorizationCodeAsync();
                lblAuthorizationCode.Text = authenticationRequestData.user_code;
                stackAuthorizationCode.Hidden = false;
                stackLoadingCode.Hidden = true;

                Repo.RDHelper.RDAgent.StartCheckingForAuthorization(authenticationRequestData);
                wvAuthorizationPage.LoadRequest(new NSUrlRequest(new NSUrl(authenticationRequestData.verification_url)));
            }
            catch (Exception)
            {
                var alert = UIAlertController.Create("Errore", "Errore: impossibile ottenere il codice di autenticazione", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (action) =>
                {
                    NavigationController.PopViewController(true);
                }));
                PresentViewController(alert, true, null);
            }
        }

        private void RDAgent_AuthenticationFailed(object sender, EventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                var alert = UIAlertController.Create("Errore", "Errore - procedura di autorizzazione fallita: tempo scaduto o altro errore di rete", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (action) =>
                {
                    NavigationController.PopViewController(true);
                }));
                PresentViewController(alert, true, null);
            });
        }

        private void RDAgent_AuthenticationSuccessful(object sender, EventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                Repo.Settings.RDToken = Repo.RDHelper.RDAgent.Token;
                var alert = UIAlertController.Create("Successo", "Account autorizzato con successo!", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (action) =>
                {
                    NavigationController.PopViewController(true);
                }));
                PresentViewController(alert, true, null);
            });
        }
    }
}
