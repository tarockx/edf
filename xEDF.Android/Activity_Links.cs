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
using xEDF.Droid.ArrayAdapters;

namespace xEDF.Droid
{
    [Activity(Label = "Link disponibili")]
    public class Activity_Links : Activity
    {
        public static List<Link> Links { get; set; }

        private LinksAdapter linksAdapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if(Links == null)
            {
                Finish();
                return;
            }

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            // Create your application here
            SetContentView(Resource.Layout.JustAListView);
            ListView listView = FindViewById<ListView>(Resource.Id.justAListView1);
            linksAdapter = new LinksAdapter(this, Links);
            listView.Adapter = linksAdapter;

            listView.ItemClick += ListView_ItemClick;
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Helpers.ActivityPusher.Push(this, Links[e.Position]);
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