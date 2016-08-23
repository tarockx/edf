using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using libEraDeiFessi;
using Android.Views.InputMethods;
using RealDebrid4DotNet.RestModel;
using static RealDebrid4DotNet.RestModel.LinkUnrestrictRequestResponse;

namespace xEDF.Droid
{
    [Activity(Label = "Link", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    [IntentFilter(new[] { Intent.ActionSend }, Label = "Sblocca via EDF", Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataMimeType = "text/plain")]
    public class Activity_LinkManager : Activity
    {
        private TextView labelUrl;
        private TextView labelLinkSupported;
        private TextView labelLinkUnsupported;
        private TextView labelUnblockedFileInfo;
        private TextView labelError;

        private Button btnUnblock;

        private LinearLayout viewErrorMessage;
        private LinearLayout viewRDAuthorize;
        private LinearLayout viewUnblockedLink;

        private LinearLayout layoutMultiLinks;
        private Spinner spinnerMultiLinks;

        private LinkUnrestrictRequestResponse response = null;
        private Alternative selectedAlternative = null;

        public static Link Link;

        private string SelectedLink
        {
            get
            {
                if (selectedAlternative != null)
                    return selectedAlternative.download;
                else
                    return response.download;
            }
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            if (Intent.Action != null && Intent.Action.Equals(Intent.ActionSend) && Intent.Type != null && Intent.Type.Equals("text/plain"))
            {
                string linkStr = Intent.GetStringExtra(Intent.ExtraText);
                if (linkStr != null)
                {
                    Link = new Link("Link esterno", linkStr);
                }
            }

            if (Link == null)
            {
                Finish();
                return;
            }

            SetContentView(Resource.Layout.LinkManager);
            Title = string.IsNullOrWhiteSpace(Link.Text) ? "Link" : "Link: " + Link.Text;

            labelUrl = FindViewById<TextView>(Resource.Id.labelUrl);
            labelLinkSupported = FindViewById<TextView>(Resource.Id.labelLinkSupported);
            labelLinkUnsupported = FindViewById<TextView>(Resource.Id.labelLinkUnsupported);
            labelUnblockedFileInfo = FindViewById<TextView>(Resource.Id.labelUnblockedFileInfo);
            labelError = FindViewById<TextView>(Resource.Id.labelError);

            viewErrorMessage = FindViewById<LinearLayout>(Resource.Id.viewErrorMessage);
            viewUnblockedLink = FindViewById<LinearLayout>(Resource.Id.viewUnblockedLink);
            viewRDAuthorize = FindViewById<LinearLayout>(Resource.Id.viewNotLoggedIn);

            btnUnblock = FindViewById<Button>(Resource.Id.btnUnblock);
            btnUnblock.Click += BtnUnblock_Click;

            Button btnAuthorizeRD = FindViewById<Button>(Resource.Id.btnAuthorizeRD);
            btnAuthorizeRD.Click += BtnAuthorizeRD_Click;

            layoutMultiLinks = FindViewById<LinearLayout>(Resource.Id.layoutMultiLinks);
            spinnerMultiLinks = FindViewById<Spinner>(Resource.Id.spinnerMultiLinks);
            spinnerMultiLinks.ItemSelected += SpinnerMultiLinks_ItemSelected;

            //other buttons
            Button btnOpenOriginal = FindViewById<Button>(Resource.Id.btnOpenOriginal);
            btnOpenOriginal.Click += BtnOpenOriginal_Click;
            ImageButton btnCopyOriginal = FindViewById<ImageButton>(Resource.Id.btnCopyOriginal);
            btnCopyOriginal.Click += BtnCopyOriginal_Click;
            ImageButton btnShareOriginal = FindViewById<ImageButton>(Resource.Id.btnShareOriginal);
            btnShareOriginal.Click += BtnShareOriginal_Click;

            Button btnOpenUnblocked = FindViewById<Button>(Resource.Id.btnOpenUnblocked);
            btnOpenUnblocked.Click += BtnOpenUnblocked_Click;
            ImageButton btnCopyUnblocked = FindViewById<ImageButton>(Resource.Id.btnCopyUnblocked);
            btnCopyUnblocked.Click += BtnCopyUnblocked_Click;
            ImageButton btnShareUnblocked = FindViewById<ImageButton>(Resource.Id.btnShareUnblocked);
            btnShareUnblocked.Click += BtnShareUnblocked_Click;

        }

        private void SpinnerMultiLinks_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (e.Position > 0)
            {
                selectedAlternative = response.alternatives[e.Position - 1];
            }
            else
            {
                selectedAlternative = null;
            }
        }



        protected override void OnStart()
        {
            base.OnStart();
            SetupUI();
        }

        private void BtnAuthorizeRD_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(Activity_AuthorizeRD));
            StartActivity(i);
        }

        private void BtnShareUnblocked_Click(object sender, EventArgs e)
        {
            Intent share = new Intent(Intent.ActionSend);
            share.SetType("text/plain");

            share.PutExtra(Intent.ExtraSubject, "Link da EDF");
            share.PutExtra(Intent.ExtraText, SelectedLink);

            StartActivity(Intent.CreateChooser(share, "Condividi link"));
        }

        private void BtnCopyUnblocked_Click(object sender, EventArgs e)
        {
            Android.Content.ClipboardManager clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
            ClipData clip = ClipData.NewPlainText("EDFlink", SelectedLink);
            clipboard.PrimaryClip = clip;

            Toast.MakeText(this, "Link copiato negli appunti", ToastLength.Short).Show();
        }

        private void BtnOpenUnblocked_Click(object sender, EventArgs e)
        {
            Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(SelectedLink));
            StartActivity(browserIntent);
        }

        private void BtnShareOriginal_Click(object sender, EventArgs e)
        {
            Intent share = new Intent(Intent.ActionSend);
            share.SetType("text/plain");

            share.PutExtra(Intent.ExtraSubject, "Link da EDF");
            share.PutExtra(Intent.ExtraText, Link.Url);

            StartActivity(Intent.CreateChooser(share, "Condividi link"));
        }

        private void BtnCopyOriginal_Click(object sender, EventArgs e)
        {
            Android.Content.ClipboardManager clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
            ClipData clip = ClipData.NewPlainText("EDFlink", Link.Url);
            clipboard.PrimaryClip = clip;

            Toast.MakeText(this, "Link copiato negli appunti", ToastLength.Short).Show();
        }

        private void BtnOpenOriginal_Click(object sender, EventArgs e)
        {
            Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(Link.Url));
            StartActivity(browserIntent);
        }

        private void BtnUnblock_Click(object sender, EventArgs e)
        {
            UnblockLink();
        }

        private async void UnblockLink()
        {
            //Show loading      
            ProgressDialog progress = ProgressDialog.Show(this, "Caricamento", "Sblocco del link in corso...");

            response = await Repo.RDHelper.RDAgent.UnrestrictLinkAsync(Link.Url);

            //hide loading
            progress.Hide();

            SetupUI();
        }

        private void SetupUI()
        {
            labelUrl.Text = Link.Url;

            if (Repo.RDHelper.LoggedIntoRD)
            {
                viewRDAuthorize.Visibility = ViewStates.Gone;

                btnUnblock.Enabled = true;

                if (Repo.RDHelper.RDAgent.LinkSupported(Link.Url))
                {
                    labelLinkSupported.Visibility = ViewStates.Visible;
                    labelLinkUnsupported.Visibility = ViewStates.Gone;
                }
                else
                {
                    labelLinkSupported.Visibility = ViewStates.Gone;
                    labelLinkUnsupported.Visibility = ViewStates.Visible;
                }

                if (response == null) //didn't try to unblock yet
                {
                    viewErrorMessage.Visibility = ViewStates.Gone;
                    viewUnblockedLink.Visibility = ViewStates.Gone;
                }
                else if (response != null && !response.has_error) //unblocked succesfully
                {
                    viewUnblockedLink.Visibility = ViewStates.Visible;
                    viewErrorMessage.Visibility = ViewStates.Gone;

                    if (response.alternatives.Count > 0)
                    {
                        string[] choices = new string[response.alternatives.Count + 1];
                        choices[0] = "[default] " + response.filename;
                        for (int i = 0; i < response.alternatives.Count; i++)
                        {
                            Alternative alternative = response.alternatives[i];
                            choices[i+1] = "[" + alternative.quality + "] " + alternative.filename;
                        }
                        var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, choices);

                        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                        spinnerMultiLinks.Adapter = adapter;

                        layoutMultiLinks.Visibility = ViewStates.Visible;
                        labelUnblockedFileInfo.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        labelUnblockedFileInfo.Text = string.Format("{0} - [{1}]", response.filename, response.FormattedFilesize);

                        layoutMultiLinks.Visibility = ViewStates.Gone;
                        labelUnblockedFileInfo.Visibility = ViewStates.Visible;
                    }

                }
                else //tried to unblock but error occurred
                {
                    viewUnblockedLink.Visibility = ViewStates.Gone;
                    viewErrorMessage.Visibility = ViewStates.Visible;

                    labelError.Text = response == null ? "N/A" : response.error_message;
                }
            }
            else
            {
                viewRDAuthorize.Visibility = ViewStates.Visible;
                viewErrorMessage.Visibility = ViewStates.Gone;
                viewUnblockedLink.Visibility = ViewStates.Gone;

                labelLinkSupported.Visibility = ViewStates.Gone;
                labelLinkUnsupported.Visibility = ViewStates.Gone;
                btnUnblock.Enabled = false;
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}