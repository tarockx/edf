using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS
{
    public class LoadingOverlay : UIView
    {
        // control declarations
        UIActivityIndicatorView activitySpinner;
        UILabel loadingLabel;
        string message = "Caricamento in corso";
        int progress = 0;
        int totalProgress = 0;

        public static LoadingOverlay Instantiate(string message, int progress, int total)
        {
            var bounds = UIScreen.MainScreen.Bounds;
            if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight)
            {
                //bounds.Size = new CGSize(bounds.Size.Height, bounds.Size.Width);
            }
            LoadingOverlay loadingOverlay = new LoadingOverlay(bounds);
            loadingOverlay.setMessage(message);
            loadingOverlay.setTotalProgress(total);
            loadingOverlay.setCurrentProgress(progress);
            return loadingOverlay;
        }

        public LoadingOverlay(CGRect frame) : base(frame)
        {
            // configurable bits
            BackgroundColor = UIColor.Black;
            Alpha = 0.75f;
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat labelHeight = 22;
            nfloat labelWidth = Frame.Width - 20;

            // derive the center x and y
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // create the activity spinner, center it horizontall and put it 5 points above center x
            activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new CGRect(
                centerX - (activitySpinner.Frame.Width / 2),
                centerY - activitySpinner.Frame.Height - 20,
                activitySpinner.Frame.Width,
                activitySpinner.Frame.Height);
            activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(activitySpinner);
            activitySpinner.StartAnimating();

            // create and configure the "Loading Data" label
            loadingLabel = new UILabel(new CGRect(
                centerX - (labelWidth / 2),
                centerY + 20,
                labelWidth,
                labelHeight
                ));
            loadingLabel.BackgroundColor = UIColor.Clear;
            loadingLabel.TextColor = UIColor.White;
            updateMessage();
            loadingLabel.TextAlignment = UITextAlignment.Center;
            loadingLabel.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(loadingLabel);

        }

        public void setTotalProgress(int progress)
        {
            totalProgress = progress;
            updateMessage();
        }

        public void setCurrentProgress(int progress)
        {
            this.progress = progress;
            updateMessage();
        }

        public void setMessage(string m)
        {
            message = m;
            updateMessage();
        }

        private object updateLock = new object();
        public void incrementProgress()
        {
            lock (updateLock)
            {
                progress += 1;
                InvokeOnMainThread(
                    delegate
                    {
                        updateMessage();
                    }
                );
            }
        }

        private void updateMessage()
        {
            if(totalProgress == 0)
                loadingLabel.Text = string.Format("{0}...", message);
            else
                loadingLabel.Text = string.Format("{0} ({1} di {2})...", message, progress.ToString(), totalProgress.ToString());
        }

        

        /// <summary>
        /// Fades out the control and then removes it from the super view
        /// </summary>
        public void Hide()
        {
            UIView.Animate(
                0.5, // duration
                () => { Alpha = 0; },
                () => { RemoveFromSuperview(); }
            );
        }
    }
}
