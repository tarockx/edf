using Foundation;
using libEraDeiFessi;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_HtmlContent : UIViewController
	{
        public HtmlContent Content { get; set; }
        public string ContentTitle { get; set; }
        public ViewController_HtmlContent (IntPtr handle) : base (handle)
		{
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Helpers.ControllerPusher.Navigator = NavigationController;
            Helpers.ControllerPusher.Storyboard = Storyboard;

            Title = ContentTitle;

            webView.LoadHtmlString(Content.Content, null);
            webView.Delegate = new UIWebViewLinkCapturingDelegate();
        }

        class UIWebViewLinkCapturingDelegate : UIWebViewDelegate
        {
            public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
            {
                if (request.Url.AbsoluteString != "about:blank" && navigationType == UIWebViewNavigationType.LinkClicked)
                {
                    Helpers.ControllerPusher.Push(new Link("", request.Url.AbsoluteString));
                    return false;
                }
                else
                    return true;
            }
        }
	}
}
