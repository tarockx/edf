using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Android.Content;

namespace xEDF.Droid
{
    [Activity(Label = "Autorizzazione Account")]
    public class Activity_AuthorizeRD : Activity
    {
        private LinearLayout viewAuthorizationCode;
        private LinearLayout viewLoadingCode;
        private TextView tvAuthorizationCode;
        private WebView wvAuthorizationPage;

        private class SelfContainedWebClient : WebViewClient
        {
            public bool shouldOverrideUrlLoading(WebView view, String url)
            {
                view.LoadUrl(url);
                return true;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.AuthorizeRD);

            tvAuthorizationCode = FindViewById<TextView>(Resource.Id.tvAuthorizationCode);

            wvAuthorizationPage = FindViewById<WebView>(Resource.Id.wvAuthorizationPage);
            wvAuthorizationPage.SetWebViewClient(new SelfContainedWebClient());
            wvAuthorizationPage.Settings.JavaScriptEnabled = true;

            viewLoadingCode = FindViewById<LinearLayout>(Resource.Id.viewLoadingCode);
            viewAuthorizationCode = FindViewById<LinearLayout>(Resource.Id.viewAuthorizationCode);
            Button btnCopyAuthorizationCode = FindViewById<Button>(Resource.Id.btnCopyAuthorizationCode);
            btnCopyAuthorizationCode.Click += BtnCopyAuthorizationCode_Click;

            viewAuthorizationCode.Visibility = ViewStates.Gone;
            viewLoadingCode.Visibility = ViewStates.Visible;

            GetAuthorizationCode();
        }

        private void BtnCopyAuthorizationCode_Click(object sender, EventArgs e)
        {
            ClipboardManager clipboard = (ClipboardManager)GetSystemService(ClipboardService);
            ClipData clip = ClipData.NewPlainText("EDFAuthorizationCode", tvAuthorizationCode.Text);
            clipboard.PrimaryClip = clip;

            Toast.MakeText(this, "Codice copiato negli appunti", ToastLength.Short).Show();
        }

        private async void GetAuthorizationCode()
        {
            try
            {
                Repo.RDHelper.RDAgent.AuthenticationSuccessful += RDAgent_AuthenticationSuccessful;
                Repo.RDHelper.RDAgent.AuthenticationFailed += RDAgent_AuthenticationFailed;

                var authenticationRequestData = await Repo.RDHelper.RDAgent.GetAuthorizationCodeAsync();
                tvAuthorizationCode.Text = authenticationRequestData.user_code;
                viewAuthorizationCode.Visibility = ViewStates.Visible;
                viewLoadingCode.Visibility = ViewStates.Gone;

                Repo.RDHelper.RDAgent.StartCheckingForAuthorization(authenticationRequestData);
                wvAuthorizationPage.LoadUrl(authenticationRequestData.verification_url);
            }
            catch (Exception)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetCancelable(false);
                builder.SetMessage("Errore: impossibile ottenere il codice di autenticazione.");
                builder.SetPositiveButton("Ok", (sender2, e2) =>
                {
                    Finish();
                });
                builder.Create().Show();
            }

        }

        private void RDAgent_AuthenticationFailed(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetCancelable(false);
                builder.SetMessage("Errore - procedura di autorizzazione fallita: tempo scaduto o altro errore di rete");
                builder.SetPositiveButton("Ok", (sender2, e2) =>
                {
                    Finish();
                });
                builder.Create().Show();
            });
        }

        private void RDAgent_AuthenticationSuccessful(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                Repo.Settings.RDToken = Repo.RDHelper.RDAgent.Token;
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetCancelable(false);
                builder.SetMessage("Account autorizzato con successo!");
                builder.SetPositiveButton("Ok", (sender2, e2) =>
                {
                    Finish();
                });
                builder.Create().Show();
            });
        }
    }
}