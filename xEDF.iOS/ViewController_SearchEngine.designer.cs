// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace xEDF.iOS
{
	[Register ("ViewController_SearchEngine")]
	partial class ViewController_SearchEngine
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnSearch { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableSearchSuggestions { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField txtSearch { get; set; }

		[Action ("BtnSearch_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void BtnSearch_TouchUpInside (UIButton sender);

		[Action ("txtSearch_ValueChanged:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void txtSearch_ValueChanged (UITextField sender);

		void ReleaseDesignerOutlets ()
		{
			if (btnSearch != null) {
				btnSearch.Dispose ();
				btnSearch = null;
			}
			if (tableSearchSuggestions != null) {
				tableSearchSuggestions.Dispose ();
				tableSearchSuggestions = null;
			}
			if (txtSearch != null) {
				txtSearch.Dispose ();
				txtSearch = null;
			}
		}
	}
}
