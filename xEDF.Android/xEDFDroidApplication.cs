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
using Android.Content.PM;

namespace xEDF.Droid
{
    [Application]
    class xEDFDroidApplication : Application
    {
        public static Context AppContext;
        public static string Version;
        public xEDFDroidApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
            AppContext = ApplicationContext;
            PackageInfo pInfo = AppContext.PackageManager.GetPackageInfo(AppContext.PackageName, 0);
            Version = pInfo.VersionName;
        }

        public override void OnCreate()
        {
            // If OnCreate is overridden, the overridden c'tor will also be called.
            base.OnCreate();

        }

    }
}