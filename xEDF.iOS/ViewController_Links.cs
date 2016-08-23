using libEraDeiFessi;
using System;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_Links : UITableViewController
	{
        private TableSource_Links linksTableSource;

        public ContentNestBlock ParentBlock { get; set; }

        public ViewController_Links (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Helpers.ControllerPusher.Navigator = NavigationController;
            Helpers.ControllerPusher.Storyboard = Storyboard;

            linksTableSource = new TableSource_Links(ParentBlock.Links);
            TableView.Source = linksTableSource;

            linksTableSource.LinkSelected += LinksTableSource_LinkSelected;

            Title = ParentBlock.Title;

            //Helpers.UIHelper.RegisterBackToSearchGesture(NavigationController, View);
        }

        private void LinksTableSource_LinkSelected(object sender, TableSource_Links.LinkSelectedEventArgs e)
        {
            Helpers.ControllerPusher.Push(e.Link);
        }
    }
}
