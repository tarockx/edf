using CoreGraphics;
using Foundation;
using libEraDeiFessi;
using RealDebrid4DotNet.RestModel;
using System;
using System.CodeDom.Compiler;
using ToastIOS;
using UIKit;
using xEDFlib;

namespace xEDF.iOS
{
    partial class ViewController_LinkManager : UIViewController
    {
        public Link Link { get; set; }

        private LinkUnrestrictRequestResponse response = null;
        private LoadingOverlay loadingOverlay;

        private UIAlertAction loginOkayAction;

        public ViewController_LinkManager(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            btnCopyOriginal.TouchUpInside += BtnCopyOriginal_TouchUpInside;
            btnOpenOriginal.TouchUpInside += BtnOpenOriginal_TouchUpInside;
            btnUnblock.TouchUpInside += BtnUnblock_TouchUpInside;
            btnCopyUnblocked.TouchUpInside += BtnCopyUnblocked_TouchUpInside;
            btnOpenUnblocked.TouchUpInside += BtnOpenUnblocked_TouchUpInside;
            btnLoginToRD.TouchUpInside += BtnLoginToRD_TouchUpInside;

            //SetupUI();

            //Helpers.UIHelper.RegisterBackToSearchGesture(NavigationController, View);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            SetupUI();
        }

        private void BtnLoginToRD_TouchUpInside(object sender, EventArgs e)
        {
            UIViewController controller = Storyboard.InstantiateViewController("RDAuthenticatorController");
            ViewController_RDAuthenticator rdAuthenticator = controller as ViewController_RDAuthenticator;
            if (rdAuthenticator != null)
            {
                this.NavigationController.PushViewController(rdAuthenticator, true);
            }
        }

        #region "Copy and Open"

        private void BtnOpenOriginal_TouchUpInside(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(Link.Url));
        }

        private void BtnCopyOriginal_TouchUpInside(object sender, EventArgs e)
        {
            UIPasteboard.General.String = Link.Url;
            Toast.MakeText("Link copiato").Show(ToastType.Info);
        }
        private void BtnOpenUnblocked_TouchUpInside(object sender, EventArgs e)
        {
            PromptOpenWith(response.download);
        }

        private void BtnCopyUnblocked_TouchUpInside(object sender, EventArgs e)
        {
            UIPasteboard.General.String = response.download;
            Toast.MakeText("Link copiato").Show(ToastType.Info);
        }
        #endregion

        private void PromptOpenWith(string link)
        {
            // Create a new Alert Controller
            UIAlertController actionSheetAlert = UIAlertController.Create("Apri con", "Scegli un'app con cui aprire questo link", UIAlertControllerStyle.ActionSheet);

            // Add Actions
            actionSheetAlert.AddAction(UIAlertAction.Create("Safari (predefinito)", UIAlertActionStyle.Default, (action) => UIApplication.SharedApplication.OpenUrl(new NSUrl(link))));

            var extraAvailableUrls = Helpers.URLHelper.GetAvailableOpenWithURLs(link);
            foreach (var item in extraAvailableUrls)
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(item.Key, UIAlertActionStyle.Default, (action) => UIApplication.SharedApplication.OpenUrl(new NSUrl(item.Value))));
            }

            actionSheetAlert.AddAction(UIAlertAction.Create("Annulla", UIAlertActionStyle.Cancel, (action) => { }));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = btnOpenUnblocked.TitleLabel;
                CGRect rect = new CGRect(btnOpenUnblocked.TitleLabel.Frame.Location, btnOpenUnblocked.TitleLabel.Frame.Size);
                rect.Inflate(10, 5);
                presentationPopover.SourceRect = btnOpenUnblocked.TitleLabel.Bounds;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                
            }

            // Display the alert
            this.PresentViewController(actionSheetAlert, true, null);
        }


        private void BtnUnblock_TouchUpInside(object sender, EventArgs e)
        {
            UnblockLink();
        }

        private async void UnblockLink()
        {
            //Show loading      
            loadingOverlay = LoadingOverlay.Instantiate("Sblocco del link, attendere", 0, 0);
            View.Add(loadingOverlay);

            response = await Repo.RDHelper.RDAgent.UnrestrictLinkAsync(Link.Url);

            //hide loading
            loadingOverlay.Hide();

            SetupUI();
        }

        

        private void SetupUI()
        {
            labelTitle.Text = Link.Text;
            labelUrl.Text = Link.Url;

            if (Repo.RDHelper.LoggedIntoRD)
            {
                viewRDLogin.Hidden = true;                

                labelNotLoggedIn.Hidden = true;
                btnUnblock.Enabled = true;

                if (Repo.RDHelper.RDAgent.LinkSupported(Link.Url))
                {
                    labelLinkSupported.Hidden = false;
                    labelLinkUnsupported.Hidden = true;
                }
                else
                {
                    labelLinkSupported.Hidden = true;
                    labelLinkUnsupported.Hidden = false;
                }

                if(response == null) //didn't try to unblock yet
                {
                    viewErrorMessage.Hidden = true;
                    viewUnblockedLink.Hidden = true;
                }
                else if (response != null && !response.has_error) //tried to unblock but error occurred
                {
                    viewUnblockedLink.Hidden = false;
                    viewErrorMessage.Hidden = true;

                    labelUnblockedFileInfo.Text = string.Format("{0} - [{1}]", response.filename, response.FormattedFilesize);
                }
                else
                {
                    viewUnblockedLink.Hidden = true;
                    viewErrorMessage.Hidden = false;

                    labelError.Text = response == null ? "N/A" : response.error_message;
                }
            }
            else
            {
                labelReasonWhyNotLoggedIn.Text = "Real-Debrid non coollegato - autorizzazione necessaria";

                viewRDLogin.Hidden = false;
                viewErrorMessage.Hidden = true;
                viewUnblockedLink.Hidden = true;

                labelNotLoggedIn.Hidden = false;
                labelLinkSupported.Hidden = true;
                labelLinkUnsupported.Hidden = true;
                btnUnblock.Enabled = false;                
            }
        }
    }
}
