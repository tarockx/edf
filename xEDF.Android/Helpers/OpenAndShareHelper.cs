using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace xEDF.Droid.Helpers
{
    public class OpenAndShareHelper
    {
        public static void Share(Context context, string url)
        {
            Intent share = new Intent(Intent.ActionSend);
            share.SetType("text/plain");

            share.PutExtra(Intent.ExtraSubject, "Link da EDF");
            share.PutExtra(Intent.ExtraText, url);

            context.StartActivity(Intent.CreateChooser(share, "Condividi link"));
        }

        public static void Open(Context context, string url)
        {
            Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            context.StartActivity(browserIntent);
        }

        public static void Copy(Context context, string url)
        {
            Android.Content.ClipboardManager clipboard = (Android.Content.ClipboardManager)context.GetSystemService(Activity.ClipboardService);
            ClipData clip = ClipData.NewPlainText("EDFlink", url);
            clipboard.PrimaryClip = clip;

            Toast.MakeText(context, "Link copiato negli appunti", ToastLength.Short).Show();
        }
    }
}