using Foundation;
using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_NestBlocks : UITableViewController
	{
        public List<NestBlock> Blocks { get; set; }
        public IEDFContentProvider Plugin { get; set; }
        public string ContentTitle { get; set; }
        private TableSource_NestBlocks nestedContentTableSource;
        public ViewController_NestBlocks (IntPtr handle) : base (handle)
		{
		}
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Helpers.ControllerPusher.Navigator = NavigationController;
            Helpers.ControllerPusher.Storyboard = Storyboard;

            Title = ContentTitle;
            nestedContentTableSource = new TableSource_NestBlocks(TableView, Blocks);
            TableView.Source = nestedContentTableSource;
            nestedContentTableSource.NestBlockSelected += NestedContentTableSource_NestBlockSelected;

            //Helpers.UIHelper.RegisterBackToSearchGesture(NavigationController, View);
        }

        private void NestedContentTableSource_NestBlockSelected(object sender, TableSource_NestBlocks.NestBlockSelectedEventArgs e)
        {
            LoadContentBlock(e.NestBlock);
        }

        private void LoadContentBlock(NestBlock block)
        {
            //UIViewController controller = Storyboard.InstantiateViewController("ContentNestBlockController");
            //ViewController_ContentNestBlock bookmarksController = controller as ViewController_ContentNestBlock;
            //if (bookmarksController != null)
            //{
            //    bookmarksController.ParentBlock = block;
            //    this.NavigationController.PushViewController(bookmarksController, true);
            //}

            Helpers.ControllerPusher.Push(block);
        }
    }
}
