using System;
using System.Collections.Generic;
using System.Text;
using UIKit;


namespace xEDF.iOS.Helpers
{
    public class UIHelper
    {
        public static void RegisterBackToSearchTwoFingerGesture(UINavigationController controller, UIView view)
        {
            Random r = new Random();
            int x = r.Next();
            var edgeTouchRecognizer = new UIScreenEdgePanGestureRecognizer();
            edgeTouchRecognizer.AddTarget(() =>
            {
                Console.WriteLine("Popping view:" + x.ToString());
                controller.PopToRootViewController(true);
            });
            edgeTouchRecognizer.Edges = UIRectEdge.Right;
            edgeTouchRecognizer.CancelsTouchesInView = false;
            edgeTouchRecognizer.MinimumNumberOfTouches = 2;
            edgeTouchRecognizer.MaximumNumberOfTouches = 2;
            view.AddGestureRecognizer(edgeTouchRecognizer);
        }

        public static void RegisterBackToSearchLongPressGesture(UINavigationController controller, UINavigationBar navBar)
        {
            var edgeTouchRecognizer = new UILongPressGestureRecognizer();
            edgeTouchRecognizer.AddTarget(() =>
            {
                nfloat height = navBar.Bounds.Size.Height;
                CoreGraphics.CGPoint pt = edgeTouchRecognizer.LocationOfTouch(0, navBar);
                CoreGraphics.CGRect rectC = new CoreGraphics.CGRect(0, 0, 100, height);
                if (rectC.Contains(pt))
                {
                    controller.PopToRootViewController(true);
                }

            });
            navBar.AddGestureRecognizer(edgeTouchRecognizer);
        }
    }
}
