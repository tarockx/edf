using Foundation;
using libEraDeiFessi;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_ContentNestBlock : UITableViewController
	{
        private TableSource_NestBlocks contentNestBlocksTableSource;

        public NestBlock ParentBlock { get; set; }
        public ViewController_ContentNestBlock (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Helpers.ControllerPusher.Navigator = NavigationController;
            Helpers.ControllerPusher.Storyboard = Storyboard;

            contentNestBlocksTableSource = new TableSource_NestBlocks(TableView, ParentBlock.Children);
            TableView.Source = contentNestBlocksTableSource;

            contentNestBlocksTableSource.ContentNestBlockSelected += ContentNestBlocksTableSource_ContentNestBlockSelected;
            Title = ParentBlock.Title;

            //Helpers.UIHelper.RegisterBackToSearchGesture(NavigationController, View);
        }

        private void ContentNestBlocksTableSource_ContentNestBlockSelected(object sender, TableSource_NestBlocks.ContentNestBlockSelectedEventArgs e)
        {
            OpenLinks(e.ContentNestBlock);
        }

        private void OpenLinks(ContentNestBlock contentNestBlock)
        {
            Helpers.ControllerPusher.Push(contentNestBlock);
        }
    }
}
