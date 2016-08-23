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
	[Register ("ViewController_RDAuthenticator")]
	partial class ViewController_RDAuthenticator
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnCopyAuthorizationCode { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblAuthorizationCode { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIStackView stackAuthorizationCode { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIStackView stackLoadingCode { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIWebView wvAuthorizationPage { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnCopyAuthorizationCode != null) {
				btnCopyAuthorizationCode.Dispose ();
				btnCopyAuthorizationCode = null;
			}
			if (lblAuthorizationCode != null) {
				lblAuthorizationCode.Dispose ();
				lblAuthorizationCode = null;
			}
			if (stackAuthorizationCode != null) {
				stackAuthorizationCode.Dispose ();
				stackAuthorizationCode = null;
			}
			if (stackLoadingCode != null) {
				stackLoadingCode.Dispose ();
				stackLoadingCode = null;
			}
			if (wvAuthorizationPage != null) {
				wvAuthorizationPage.Dispose ();
				wvAuthorizationPage = null;
			}
		}
	}
}
