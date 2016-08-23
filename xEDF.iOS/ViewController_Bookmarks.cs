using Foundation;
using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using ToastIOS;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_Bookmarks : UIViewController
	{
        public SearchResult SearchResult { get; set; }
        public IEDFSearch Plugin { get; set; }

        private TableSource_Bookmarks bookmarksTableSource;
        private LoadingOverlay loadingOverlay;
        public ViewController_Bookmarks (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Helpers.ControllerPusher.Navigator = NavigationController;
            Helpers.ControllerPusher.Storyboard = Storyboard;

            bookmarksTableSource = new TableSource_Bookmarks(tableBookmarks);
            tableBookmarks.Source = bookmarksTableSource;
            bookmarksTableSource.AddBookmarks(SearchResult.Result);

            bookmarksTableSource.BookmarkSelected += BookmarksTableSource_BookmarkSelected;

            CheckMoreButtonEnabledStatus();
            Title = (Plugin as IEDFPlugin).pluginName;

            //Helpers.UIHelper.RegisterBackToSearchGesture(NavigationController, View);            
        }

        partial void btnSearch_TouchUpInside(UIButton sender)
        {
            LoadMoreResults();
        }

        private void BookmarksTableSource_BookmarkSelected(object sender, TableSource_Bookmarks.BookmarkSelectedEventArgs e)
        {
            LoadContentFromBookmark(e.Bookmark);
        }

        private void CheckMoreButtonEnabledStatus()
        {
            btnLoadMore.Enabled = SearchResult != null && !string.IsNullOrEmpty(SearchResult.NextPageUrl);
        }

        private async void LoadMoreResults()
        {
            //Show loading      
            loadingOverlay = LoadingOverlay.Instantiate("Caricamento risultati", 0, 0);
            View.Add(loadingOverlay);

            SearchResult = await ParsingHelpers.GetResultPageAsync(Plugin, SearchResult.NextPageUrl);

            if (SearchResult != null && !SearchResult.HasError && SearchResult.Result.Count > 0)
                bookmarksTableSource.AddBookmarks(SearchResult.Result);

            //hide loading
            loadingOverlay.Hide();
        }

        private async void LoadContentFromBookmark(Bookmark bookmark)
        {
            //Show loading      
            loadingOverlay = LoadingOverlay.Instantiate("Recupero contenuto", 0, 0);
            View.Add(loadingOverlay);

            ParseResult result = await ParsingHelpers.ParsePageAsync(Plugin as IEDFContentProvider, bookmark);

            //hide loading
            loadingOverlay.Hide();

            if (result == null || result.HasError)
            {
                //Error occurred

                //Create Alert
                var okAlertController = UIAlertController.Create("Errore", "Impossibile recuperare il contenuto. Errore:\n\n" + result == null ? "sconosciuto" : result.ErrMsg, UIAlertControllerStyle.Alert);
                okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                UIPopoverPresentationController presentationPopover = okAlertController.PopoverPresentationController;
                if (presentationPopover != null)
                {
                    presentationPopover.SourceView = this.View;
                    presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                }

                PresentViewController(okAlertController, true, null);
            }
            else
            {
                //Loaded correctly

                switch (result.ResultType)
                {
                    case ParseResult.ParseResultType.HtmlContent:
                        //Toast.MakeText("Errore: i contenuti di tipo 'HTML puro' non sono ancora supportati. Forse nella prossima versione...", Toast.LENGTH_LONG).Show(ToastType.Error);
                        Helpers.ControllerPusher.Push(result.Result as HtmlContent, bookmark.Name);
                        break;
                    case ParseResult.ParseResultType.NestedContent:
                        Helpers.ControllerPusher.Push(result.Result as NestedContent, bookmark.Name, Plugin as IEDFPlugin);
                        break;
                    case ParseResult.ParseResultType.TorrentContent:
                        Helpers.ControllerPusher.Push(result.Result as TorrentContent);
                        break;
                    default:
                        break;
                }
            }

            

            
        }

        private void SwitchToNestedContent(NestedContent result, string title)
        {
            //Helpers.ControllerPusher.Push(result, title, Plugin as IEDFPlugin);

            //var blocks = result.GetNestBlocksWithDirectContent();
            //if(blocks.Count > 1)
            //{
            //    //More than one season/container. Let user chose
            //    UIViewController controller = Storyboard.InstantiateViewController("NestBlocksController");
            //    ViewController_NestBlocks nestBlocksController = controller as ViewController_NestBlocks;
            //    if (nestBlocksController != null)
            //    {
            //        nestBlocksController.Blocks = blocks;
            //        nestBlocksController.Plugin = Plugin as IEDFContentProvider;
            //        nestBlocksController.ContentTitle = title;
            //        this.NavigationController.PushViewController(nestBlocksController, true);
            //    }
            //}
            //else if(blocks.Count == 1)
            //{
            //    //Only one, direct push
            //    UIViewController controller = Storyboard.InstantiateViewController("ContentNestBlockController");
            //    ViewController_ContentNestBlock bookmarksController = controller as ViewController_ContentNestBlock;
            //    if (bookmarksController != null)
            //    {
            //        bookmarksController.ParentBlock = blocks[0];
            //        this.NavigationController.PushViewController(bookmarksController, true);
            //    }
            //}

            
        }
    }
}
