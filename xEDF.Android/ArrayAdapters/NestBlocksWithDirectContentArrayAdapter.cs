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
using Android.Graphics;

namespace xEDF.Droid.ArrayAdapters
{
    public class NestBlocksWithDirectContentArrayAdapter : BaseExpandableListAdapter
    {
        // Context, usually set to the activity:
        private readonly Context _context;

        // List of produce objects ("vegetables", "fruits", "herbs"):
        private List<NestBlock> blocks;


        public NestBlocksWithDirectContentArrayAdapter(Context context, List<NestBlock> content)
        {
            _context = context;
            this.blocks = content;
        }

        public override bool HasStableIds
        {
            // Indexes are used for IDs:
            get { return true; }
        }

        //---------------------------------------------------------------------------------------
        // Group methods:

        public override long GetGroupId(int groupPosition)
        {
            // The index of the group is used as its ID:
            return groupPosition;
        }

        public override int GroupCount
        {
            get { return blocks.Count; }
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            // Recycle a previous view if provided:
            var view = convertView;

            // If no recycled view, inflate a new view as a simple expandable list item 1:
            if (view == null)
            {
                var inflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                view = inflater.Inflate(Android.Resource.Layout.SimpleExpandableListItem2, null);
            }

            NestBlock block = blocks[groupPosition];

            TextView textView = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            textView.Text = block.Title;

            textView = view.FindViewById<TextView>(Android.Resource.Id.Text2);
            textView.Text = string.Format("{0} episodi/file disponibili", block.Children.Count.ToString());

            return view;
        }

        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return null;
        }

        //---------------------------------------------------------------------------------------
        // Child methods:

        public override long GetChildId(int groupPosition, int childPosition)
        {
            // The index of the child is used as its ID:
            return childPosition;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            // Return the number of children
            try
            {
                return blocks[groupPosition].Children.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            // Recycle a previous view if provided:
            View view = convertView;

            // If no recycled view, inflate a new view as a simple expandable list item 2:
            if (view == null)
            {
                var inflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                view = inflater.Inflate(Android.Resource.Layout.SimpleExpandableListItem2, null);
            }

            view.SetBackgroundColor(new Color(60, 60, 60));

            ContentNestBlock block = blocks[groupPosition].Children[childPosition] as ContentNestBlock;

            TextView textView = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            textView.Text = block.Title;

            textView = view.FindViewById<TextView>(Android.Resource.Id.Text2);
            textView.Text = string.Format("{0} link disponibili", block.Links.Count);

            return view;
        }


        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            return null;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }
    }
}