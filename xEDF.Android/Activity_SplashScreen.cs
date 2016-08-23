using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace xEDF.Droid
{
    [Activity(Theme = "@style/Theme.Splash", NoHistory = true, MainLauncher = true, Icon = "@drawable/app_icon_round")]
    public class Activity_SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            StartActivity(typeof(Activity_SearchEngine));
        }
    }
}