using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using xEDF.Droid.ArrayAdapters;
using libEraDeiFessi.Plugins;
using libEraDeiFessi;
using System.Threading.Tasks;
using Refractored.Xam.Vibrate;

namespace xEDF.Droid
{
    [Activity(Label = "xEDF", Icon = "@drawable/app_icon_round")]
    public class Activity_SearchEngine : Activity
    {
        List<String> searchSuggestions = new List<string>();
        StringListArrayAdapter listViewSearchSuggestionsAdapter;
        EditText txtSearch;
        Button btnSearch;

        ProgressDialog progress;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.SearchEngine);

            var listViewSearchSuggestions = FindViewById<ListView>(Resource.Id.listViewSearchSuggestions);
            listViewSearchSuggestionsAdapter = new StringListArrayAdapter(this, searchSuggestions);
            listViewSearchSuggestions.Adapter = listViewSearchSuggestionsAdapter;
            listViewSearchSuggestions.ItemClick += ListViewSearchSuggestions_ItemClick;
            listViewSearchSuggestions.ItemLongClick += ListViewSearchSuggestions_ItemLongClick;

            //Binding events
            txtSearch = FindViewById<EditText>(Resource.Id.txtSearch);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.EditorAction += TxtSearch_EditorAction;

            btnSearch = FindViewById<Button>(Resource.Id.btnSearch);
            btnSearch.Click += BtnSearch_Click;
            btnSearch.Enabled = false;

            //TEMP / DEBUG
            //Link l = new Link("Rapidgator", "http://www.nowvideo.li/video/7ff91f7ef7617");
            //Helpers.ActivityPusher.Push(this, l);

            if (Repo.Settings.CheckForUpdates)
                Repo.Updater.CheckForUpdatesAndPrompt(this);
        }

        protected override void OnResume()
        {
            base.OnResume();

            txtSearch.SetSelection(txtSearch.Text.Length);
        }

        private void ListViewSearchSuggestions_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            CrossVibrate.Current.Vibration(400);
            txtSearch.Text = searchSuggestions[e.Position];
            txtSearch.SetSelection(txtSearch.Text.Length);
        }

        private void TxtSearch_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if ((e.ActionId == Android.Views.InputMethods.ImeAction.Search || (int)e.ActionId == 999) && txtSearch.Text.Trim().Length > 0)
                Search(txtSearch.Text.Trim());
        }

        private void ListViewSearchSuggestions_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            txtSearch.Text = searchSuggestions[e.Position];
            Search(searchSuggestions[e.Position]);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Trim().Length > 0)
                Search(txtSearch.Text.Trim());
        }

        private void TxtSearch_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            btnSearch.Enabled = txtSearch.Text.Trim().Length > 0;
            getSuggestions();
        }



        private string currentSearch = "";
        private async void getSuggestions()
        {
            string query = txtSearch.Text.Trim();
            if (txtSearch.Text.Length >= 2)
            {
                currentSearch = query;
                List<string> suggestions = await xEDFlib.Helpers.GoogleSuggestionsProvider.DoSearchAsync(query);
                if (currentSearch == query)
                {
                    searchSuggestions.Clear();
                    searchSuggestions.AddRange(suggestions);
                }
            }
            else
            {
                searchSuggestions.Clear();
            }
            listViewSearchSuggestionsAdapter.NotifyDataSetChanged();
        }


        private async void Search(string searchQuery)
        {
            var enabledPlugins = new List<string>(PluginRepo.Plugins.Keys);
            foreach (var item in Repo.Settings.DisabledPlugins)
            {
                enabledPlugins.Remove(item);
            }

            //Show loading      
            btnSearch.Enabled = false;
            progress = new ProgressDialog(this);
            progress.SetTitle("Caricamento");
            progress.SetMessage("Ricerca in corso...");
            progress.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progress.Max = enabledPlugins.Count == 0 ? PluginRepo.Plugins.Count : enabledPlugins.Count;
            progress.Progress = 0;
            progress.Show();

            Dictionary<IEDFPlugin, SearchResult> results = await PerformSearchAsync(searchQuery, enabledPlugins);

            //hide loading
            progress.Dismiss();
            btnSearch.Enabled = true;

            //Change Activity
            Activity_SearchResults.results = results;
            Intent i = new Intent(this, typeof(Activity_SearchResults));
            StartActivity(i);
        }

        private Task<Dictionary<IEDFPlugin, SearchResult>> PerformSearchAsync(string searchQuery, List<string> enabledPlugins)
        {
            return Task<Dictionary<IEDFPlugin, SearchResult>>.Factory.StartNew(() => PerformSearch(searchQuery, enabledPlugins));
        }

        private Dictionary<IEDFPlugin, SearchResult> PerformSearch(string searchQuery, List<string> enabledPlugins)
        {
            //perform search
            Dictionary<IEDFPlugin, SearchResult> searchResults = new Dictionary<IEDFPlugin, SearchResult>();

            Parallel.ForEach(PluginRepo.Plugins.Values, new ParallelOptions { MaxDegreeOfParallelism = 4 }, plugin =>
            {
                if (enabledPlugins.Count == 0 || enabledPlugins.Contains(plugin.pluginID))
                {
                    IEDFSearch searchplugin = plugin as IEDFSearch;
                    if (searchplugin != null)
                    {
                        SearchResult res = searchplugin.PerformSearch(searchQuery);
                        lock (searchResults)
                        {
                            searchResults.Add(plugin, res);
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        progress.Progress += 1;
                    });
                }
            });

            return searchResults;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.SearchEngineMenu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menuItemSettings:
                    Intent i = new Intent(this, typeof(Activity_Options));
                    StartActivity(i);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}

