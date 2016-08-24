using libEraDeiFessi.Plugins;
using log4net;
using log4net.Config;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

[assembly: XmlConfigurator(Watch = true)]

namespace EraDeiFessi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        public static bool SHOW_DEBUG_MESSAGES { get; private set; }
        public static ILog Logger { get; private set; }


        private const string Unique = "EraDeiFessi";
        [STAThread]
        public static void Main()
        {            
            Logger = log4net.LogManager.GetLogger(typeof(App));
            

            SHOW_DEBUG_MESSAGES = Environment.GetCommandLineArgs().Contains("--SHOW_DEBUG_MESSAGES");

            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }


        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Repository.Repo.SaveSettings();

            try
            {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EraDeiFessi\\subtitles\\";
                if (System.IO.Directory.Exists(dir))
                    System.IO.Directory.Delete(dir, true);
            }
            catch { }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string updatepack = Constants.UpdateAppdataFolder + "\\update.zip";
            if (System.IO.File.Exists(updatepack))
            {
                Updater.Updater.DecompressUpdater();

                //System.Diagnostics.Process.Start(Constants.UpdateAppdataFolder + "\\EDFUpdater.exe", Constants.ProgramLocation);

                Updater.Updater.InvokeUpdater();

                Shutdown();
                return;
            }

            //Loads plugins
            PluginLoader.LoadPlugins(Constants.PluginsLocation);

            //Show main ui
            MainWindow w = new MainWindow();
            this.MainWindow = w;
            w.Show(); 
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            try
            {
                MainWindow w = this.MainWindow as MainWindow;
                w.RestoreAndShow();
            }
            catch (Exception)
            {
            }
            return true;
        }
    }
}
