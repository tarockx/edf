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
using libEraDeiFessi;
using Android.Webkit;

namespace xEDF.Droid
{
    [Activity(Label = "Activity_HtmlContent")]
    public class Activity_HtmlContent : Activity
    {
        public static HtmlContent Content { get; set; }
        public static string BookmarkTitle { get; set; }

        class LinkCapturingWebViewClient : WebViewClient
        {
            Activity_HtmlContent activity;

            public LinkCapturingWebViewClient(Activity_HtmlContent parent)
            {
                activity = parent;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                Link link = new Link("", url);
                Helpers.ActivityPusher.Push(activity, link);
                return true;
            }

            public override void OnLoadResource(WebView view, string url)
            {
                base.OnLoadResource(view, url);
                //Link link = new Link("", url);
                //Helpers.ActivityPusher.Push(activity, link);
            }

        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            if (Content == null)
            {
                Finish();
                return;
            }

            // Create your application here
            SetContentView(Resource.Layout.HtmlContent);

            Title = string.IsNullOrWhiteSpace(BookmarkTitle) ? "Link disponibili" : BookmarkTitle;

            WebView wv = FindViewById<WebView>(Resource.Id.mainWebView);
            LinkCapturingWebViewClient wc = new LinkCapturingWebViewClient(this);
            wv.SetWebViewClient(wc);

            wv.LoadDataWithBaseURL("", Content.Content, "text/html", "UTF-8", "");
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