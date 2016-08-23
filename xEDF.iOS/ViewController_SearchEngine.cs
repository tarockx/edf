using CoreGraphics;
using Foundation;
using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using Refractored.Xam.Vibrate;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace xEDF.iOS
{
    partial class ViewController_SearchEngine : UIViewController
    {
        TableSource_Suggestions searchSuggestions;
        LoadingOverlay loadingOverlay;

        public ViewController_SearchEngine searchEngineControllerInstance { get; set; }

        public ViewController_SearchEngine(IntPtr handle) : base(handle)
        {
            searchEngineControllerInstance = this;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Helpers.ControllerPusher.Navigator = NavigationController;
            Helpers.ControllerPusher.Storyboard = Storyboard;

            searchSuggestions = new TableSource_Suggestions(tableSearchSuggestions);
            tableSearchSuggestions.Source = searchSuggestions;
            searchSuggestions.SuggestionSelected += SearchSuggestions_SuggestionSelected;
            btnSearch.Enabled = false;

            txtSearch.ShouldReturn = (t) =>
            {
                if (txtSearch.Text.Length > 0)
                    Search(txtSearch.Text.Trim());
                return true;
            };

            Helpers.UIHelper.RegisterBackToSearchLongPressGesture(NavigationController, NavigationController.NavigationBar);

            this.NavigationItem.SetRightBarButtonItem(
                new UIBarButtonItem(UIImage.FromFile("ic_settings.png"), UIBarButtonItemStyle.Plain, (sender, args) =>
                {
                    UIViewController controller = Storyboard.InstantiateViewController("OptionsController");
                    ViewController_Options oprionsController = controller as ViewController_Options;
                    if (oprionsController != null)
                    {
                        this.NavigationController.PushViewController(oprionsController, true);
                    }
                })
                , true);

            if (Repo.Settings.CheckForUpdates)
                Repo.Updater.CheckForUpdatesAndPrompt(this);
            //TEMP / DEBUG
            //Link l = new Link("Rapidgator", "http://www.nowvideo.li/video/7ff91f7ef7617");
            //Helpers.ControllerPusher.Push(l);
        }


        partial void txtSearch_ValueChanged(UITextField sender)
        {
            getSuggestions();
        }

        private string currentSearch = "";
        private async void getSuggestions()
        {
            btnSearch.Enabled = txtSearch.Text.Length > 0;
            string query = txtSearch.Text.Trim();
            if (txtSearch.Text.Length >= 2)
            {
                currentSearch = query;
                List<string> suggestions = await xEDFlib.Helpers.GoogleSuggestionsProvider.DoSearchAsync(query);
                if (currentSearch == query)
                    searchSuggestions.UpdateSuggestions(suggestions);
            }
            else
            {
                searchSuggestions.UpdateSuggestions(null);
            }
        }

        partial void BtnSearch_TouchUpInside(UIButton sender)
        {
            if (txtSearch.Text.Trim().Length > 0)
                Search(txtSearch.Text.Trim());
        }

        private void SearchSuggestions_SuggestionSelected(object sender, TableSource_Suggestions.SuggestionSelectedEventArgs e)
        {
            txtSearch.Text = e.Suggestion;
            if (e.LongClick)
            {
                CrossVibrate.Current.Vibration(400);
            }
            else
            {
                Search(e.Suggestion);
            }
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
            txtSearch.ResignFirstResponder();
            loadingOverlay = LoadingOverlay.Instantiate("Ricerca in corso", 0, enabledPlugins.Count == 0 ? PluginRepo.Plugins.Count : enabledPlugins.Count);
            View.Add(loadingOverlay);


            Dictionary<IEDFPlugin, SearchResult> results = await PerformSearchAsync(searchQuery, enabledPlugins);

            //hide loading
            loadingOverlay.Hide();
            btnSearch.Enabled = true;

            //Change View
            UIViewController controller = Storyboard.InstantiateViewController("SearchResultsController");
            ViewController_SearchResults searchResults = controller as ViewController_SearchResults;
            if (searchResults != null)
            {
                searchResults.Results = results;
                this.NavigationController.PushViewController(searchResults, true);
            }
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
                    loadingOverlay.incrementProgress();
                }
            });

            return searchResults;
        }

    }
}
