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
	[Register ("ViewController_SearchResults")]
	partial class ViewController_SearchResults
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableResults { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (tableResults != null) {
				tableResults.Dispose ();
				tableResults = null;
			}
		}
	}
}
