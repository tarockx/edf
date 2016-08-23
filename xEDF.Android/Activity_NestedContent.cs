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
using libEraDeiFessi.Plugins;
using xEDF.Droid.ArrayAdapters;
using xEDF.Droid.Helpers;

namespace xEDF.Droid
{
    [Activity(Label = "Activity_NestedContent")]
    public class Activity_NestedContent : Activity
    {
        private NestBlocksWithDirectContentArrayAdapter nestBlocksWithDirectContentArrayAdapter;

        public static NestedContent content { get; set; }
        public static string title { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            if (content == null)
            {
                Finish();
                return;
            }

            SetContentView(Resource.Layout.JustAnExpandableListView);
            ExpandableListView expandableListView = FindViewById<ExpandableListView>(Resource.Id.justAnExpandableListView1);
            nestBlocksWithDirectContentArrayAdapter = new NestBlocksWithDirectContentArrayAdapter(this, content.GetNestBlocksWithDirectContent());
            expandableListView.SetAdapter(nestBlocksWithDirectContentArrayAdapter);
            expandableListView.ChildClick += ExpandableListView_ChildClick;

            if (content.Children.Count == 1)
                expandableListView.ExpandGroup(0);

            Title = title;

        }

        private void ExpandableListView_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
            ContentNestBlock parentBlock = content.Children[e.GroupPosition].Children[e.ChildPosition] as ContentNestBlock;
            ActivityPusher.Push(this, parentBlock);
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