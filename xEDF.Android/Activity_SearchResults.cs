using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using libEraDeiFessi.Plugins;
using libEraDeiFessi;
using xEDF.Droid.ArrayAdapters;
using Refractored.Xam.Vibrate;

namespace xEDF.Droid
{
    [Activity(Label = "Risultati")]
    public class Activity_SearchResults : Activity
    {
        public static Dictionary<IEDFPlugin, SearchResult> results;
        private SearchResultsAndBookmarksArrayAdapter searchResultsAndBookmarksArrayAdapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (results == null)
            {
                Finish();
                return;
            }


            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            // Create your application here
            SetContentView(Resource.Layout.JustAnExpandableListView);

            ExpandableListView listView = FindViewById<ExpandableListView>(Resource.Id.justAnExpandableListView1);
            searchResultsAndBookmarksArrayAdapter = new SearchResultsAndBookmarksArrayAdapter(this, results);
            listView.SetAdapter(searchResultsAndBookmarksArrayAdapter);

            listView.ChildClick += ListView_ChildClick;
            listView.ItemLongClick += ListView_ItemLongClick;
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (ExpandableListView.GetPackedPositionType(e.Id) == PackedPositionType.Child)
            {
                e.Handled = true;
                CrossVibrate.Current.Vibration(400);

                IExpandableListAdapter adapter = ((ExpandableListView)e.Parent).ExpandableListAdapter;
                long packedPos = ((ExpandableListView)e.Parent).GetExpandableListPosition(e.Position);
                int groupPosition = ExpandableListView.GetPackedPositionGroup(packedPos);
                int childPosition = ExpandableListView.GetPackedPositionChild(packedPos);

                try
                {
                    Bookmark bm = results[searchResultsAndBookmarksArrayAdapter.sortedPlugins[groupPosition]].Result.ElementAt(childPosition);
                    if (bm == null)
                        return;

                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                    EventHandler<DialogClickEventArgs> handler = (hsender, he) =>
                    {
                        switch (he.Which)
                        {
                            case 0:
                                Helpers.OpenAndShareHelper.Open(this, bm.Url);
                                break;
                            case 1:
                                Helpers.OpenAndShareHelper.Copy(this, bm.Url);
                                break;
                            case 2:
                                Helpers.OpenAndShareHelper.Share(this, bm.Url);
                                break;
                            default:
                                break;
                        }
                    };
                    builder.SetCancelable(true);
                    builder.SetTitle("Altre opzioni");
                    builder.SetItems(new string[] { "Apri nel browser", "Copia link", "Condividi" }, handler);
                    builder.Create().Show();
                }
                catch { }
            }
        }

        private void ListView_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
            Bookmark bm = searchResultsAndBookmarksArrayAdapter.GetBookmark(e.GroupPosition, e.ChildPosition);
            if (bm != null)
            {
                LoadContentFromBookmark(bm);
            }
        }

        private async void LoadContentFromBookmark(Bookmark bookmark)
        {
            //Show loading      
            var progress = ProgressDialog.Show(this, "Caricamento", "Recupero contenuto...");

            var Plugin = (from p in results.Keys where p.pluginID == bookmark.PluginID select p).ElementAt(0);
            ParseResult result = await ParsingHelpers.ParsePageAsync(Plugin as IEDFContentProvider, bookmark);

            //hide loading
            progress.Dismiss();

            if (result == null || result.HasError)
            {
                //Error occurred
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetMessage("Errore")
                       .SetTitle("Impossibile recuperare il contenuto. Errore:\n\n" + result == null ? "sconosciuto" : result.ErrMsg);
                AlertDialog dialog = builder.Create();
                dialog.Show();

            }
            else
            {
                //Loaded correctly
                switch (result.ResultType)
                {
                    case ParseResult.ParseResultType.HtmlContent:
                        Helpers.ActivityPusher.Push(this, result.Result as HtmlContent, bookmark.Name);
                        break;
                    case ParseResult.ParseResultType.NestedContent:
                        Helpers.ActivityPusher.Push(this, result.Result as NestedContent, bookmark.Name);
                        break;
                    case ParseResult.ParseResultType.TorrentContent:
                        Helpers.ActivityPusher.Push(this, result.Result as TorrentContent);
                        break;
                    default:
                        break;
                }
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