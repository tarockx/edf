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
    public class SearchResultsAndBookmarksArrayAdapter : BaseExpandableListAdapter
    {
        // Context, usually set to the activity:
        private readonly Context _context;

        // List of produce objects ("vegetables", "fruits", "herbs"):
        private Dictionary<IEDFPlugin, SearchResult> _results;
        public List<IEDFPlugin> sortedPlugins;

        public SearchResultsAndBookmarksArrayAdapter(Context context, Dictionary<IEDFPlugin, SearchResult> results)
        {
            _context = context;
            _results = results;

            sortedPlugins = new List<IEDFPlugin>(results.Keys);
            sortedPlugins.Sort((x, y) => x.pluginName.CompareTo(y.pluginName));
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
            get { return sortedPlugins.Count; }
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

            IEDFPlugin plugin = sortedPlugins[groupPosition];
            // Get the built-in first text view and insert the group name ("Vegetables", "Fruits", etc.):
            TextView textView = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            textView.Text = plugin.pluginName;

            textView = view.FindViewById<TextView>(Android.Resource.Id.Text2);

            SearchResult res = _results[plugin];
            if (res == null || res.HasError || res.Result == null || res.Result.Count == 0)
            {
                textView.Text = "Nessun risultato trovato";
                textView.SetTextColor(Color.Red);
            }
            else
            {
                textView.Text = "Risultati trovati: " + res.Result.Count.ToString();
                textView.SetTextColor(Color.Green);
            }

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
            SearchResult res = _results[sortedPlugins[groupPosition]];
            if (res == null || res.HasError || res.Result == null || res.Result.Count == 0)
                return 0;
            else
                return res.Result.Count + (string.IsNullOrWhiteSpace(res.NextPageUrl) ? 0 : 1);
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            // Recycle a previous view if provided:
            View view = null;



            var res = _results[sortedPlugins[groupPosition]];

            if (childPosition < res.Result.Count)
            {
                // If no recycled view, inflate a new view as a simple expandable list item 2:
                if (view == null)
                {
                    var inflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    view = inflater.Inflate(Android.Resource.Layout.SimpleExpandableListItem2, null);
                }

                view.SetBackgroundColor(new Color(60, 60, 60));

                var bkm = res.Result.ElementAt(childPosition);

                // Get the built-in first text view and insert the child name
                TextView textView = view.FindViewById<TextView>(Android.Resource.Id.Text1);
                textView.Text = bkm.Name;
                textView.SetMaxLines(2);

                // Reuse the textView to insert the number of produce units into the child's second text field:
                textView = view.FindViewById<TextView>(Android.Resource.Id.Text2);
                textView.Text = bkm.Subtitle;
                textView.SetMaxLines(2);
            }
            else
            {
                View prev = GetChildView(groupPosition, childPosition - 1, isLastChild, convertView, parent);

                LinearLayout layout = new LinearLayout(_context);
                layout.SetBackgroundColor(new Color(60, 60, 60));

                Button b = new Button(_context);
                b.Text = "Carica altri risultati";
                b.Click += delegate
                {
                    LoadMoreResults(sortedPlugins[groupPosition]);
                };
                //b.SetPadding(prev.PaddingLeft, prev.PaddingTop, prev.PaddingRight, prev.PaddingBottom);

                LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                p.SetMargins(prev.PaddingLeft, prev.PaddingTop, prev.PaddingRight, prev.PaddingBottom);

                layout.AddView(b, p);

                view = layout;
            }

            return view;
        }

        private async void LoadMoreResults(IEDFPlugin Plugin)
        {
            var progress = ProgressDialog.Show(_context, "Caricamento", "Recupero ulteriori risultati...");
            var result = _results[Plugin];

            SearchResult SearchResult = await ParsingHelpers.GetResultPageAsync(Plugin as IEDFSearch, result.NextPageUrl);
            result.NextPageUrl = SearchResult.NextPageUrl;

            if (SearchResult != null && !SearchResult.HasError && SearchResult.Result.Count > 0)
                foreach (var item in SearchResult.Result)
                {
                    result.Result.Add(item);
                }

            NotifyDataSetChanged();
            progress.Dismiss();
        }

        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            return null;
        }

        public Bookmark GetBookmark(int groupPosition, int childPosition)
        {
            try
            {
                return _results[sortedPlugins[groupPosition]].Result.ElementAt(childPosition);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }
    }
}