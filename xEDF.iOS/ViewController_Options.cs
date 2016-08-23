using Foundation;
using libEraDeiFessi.Plugins;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using UIKit;

namespace xEDF.iOS
{
	partial class ViewController_Options : UIViewController
	{
        private TableSource_Options optionsTableSource;

		public ViewController_Options (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            optionsTableSource = new TableSource_Options(PluginRepo.getPluginsSorted());
            tableOptions.Source = optionsTableSource;
        }
    }
}
