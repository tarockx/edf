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
        public static ILog Logger { get; private set; } = LogManager.GetLogger(typeof(App));


        private const string Unique = "EraDeiFessi";
        [STAThread]
        public static void Main()
        {            
            SHOW_DEBUG_MESSAGES = Environment.GetCommandLineArgs().Contains("--SHOW_DEBUG_MESSAGES");

            Logger.Debug("Initializing single-instance application");
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                Logger.Debug("Initializing App's XAML components");

                application.InitializeComponent();

                Logger.Debug("Running main application loop...");
                application.Run();

                // Allow single instance code to perform cleanup operations
                Logger.Debug("App's main loop ending, cleaning up...");
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
            Logger.Debug("Checking for updated package");
            if (System.IO.File.Exists(updatepack))
            {
                Logger.Debug("Update found and ready for installation! Decompressing updater...");
                Updater.Updater.DecompressUpdater();

                Logger.Debug("Invoking updater, EDF will shut down after this");
                Updater.Updater.InvokeUpdater();

                Shutdown();
                return;
            }

            //Loads plugins
            Logger.Debug("Loading EDF Plugins...");
            PluginLoader.LoadPlugins(Constants.PluginsLocation);
            Logger.Debug(PluginsRepo.Plugins.Count.ToString() + " EDF plugins loaded succesfully!");

            //Show main ui
            Logger.Debug("Instantiating MainWindow");
            MainWindow w = new MainWindow();
            this.MainWindow = w;
            Logger.Debug("Showing MainWindow...");
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
