using Foundation;
using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_SearchResults : UIViewController
	{
        public Dictionary<IEDFPlugin, SearchResult> Results { get; set; }
        private TableSource_SearchResults searchResultsTableSource;

        public ViewController_SearchResults (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Helpers.ControllerPusher.Navigator = NavigationController;
            Helpers.ControllerPusher.Storyboard = Storyboard;

            searchResultsTableSource = new TableSource_SearchResults(tableResults, Results);
            tableResults.Source = searchResultsTableSource;

            searchResultsTableSource.SearchResultSelected += SearchResultsTableSource_SearchResultSelected;

            //Helpers.UIHelper.RegisterBackToSearchGesture(NavigationController, View);
        }

        private void SearchResultsTableSource_SearchResultSelected(object sender, TableSource_SearchResults.SearchResultSelectedEventArgs e)
        {
            LoadBookmarks(e.SearchResult, e.Plugin);
        }

        private void LoadBookmarks(SearchResult searchResult, IEDFPlugin plugin)
        {
            if(searchResult == null || searchResult.Result == null || searchResult.Result.Count == 0)
            {
                ToastIOS.Toast.MakeText("Nessun risultato disponibile per questo sito", ToastIOS.Toast.LENGTH_LONG).Show(ToastIOS.ToastType.Warning);
                return;
            }

            List<Bookmark> bookmarks = new List<Bookmark>();
            bookmarks.AddRange(searchResult.Result);
            
            UIViewController controller = Storyboard.InstantiateViewController("BookmarksController");
            ViewController_Bookmarks bookmarksController = controller as ViewController_Bookmarks;
            if (bookmarksController != null)
            {
                bookmarksController.SearchResult = searchResult;
                bookmarksController.Plugin = plugin as IEDFSearch;
                this.NavigationController.PushViewController(bookmarksController, true);
            }
        }
    }
}
