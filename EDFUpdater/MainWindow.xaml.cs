using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Ionic.Zip;

namespace EDFUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string ProgramLocation { get { var exe = Assembly.GetExecutingAssembly().Location; return exe.Substring(0, exe.LastIndexOf("\\")); } }
        public static string Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        private string EDFLocation;
        private bool legacymode = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string arg = string.Empty;
            foreach (var item in Environment.GetCommandLineArgs())
            {
                if (item.Contains("EDFPath="))
                {
                    arg = item.Replace("EDFPath=", "").Replace("\"", "");
                    break;
                }

            }
            if (string.IsNullOrEmpty(arg))
            {
                if (File.Exists(ProgramLocation + "\\EraDeiFessi.exe"))
                { //legacy mode
                    EDFLocation = ProgramLocation;
                    legacymode = true;
                }
                else
                {
                    MessageBox.Show("Errore: il percorso di installazione di EDF non è stato fornito alla linea di comando. Riavviare l'updater con i giusti parametri", "Errore nei parametri", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
            else { 
                if (!File.Exists(arg + "\\EraDeiFessi.exe"))
                {
                    MessageBox.Show("Errore: il percorso di installazione di EDF fornito non è corretto ("+ arg +"). Riavviare l'updater con i giusti parametri", "Errore nei parametri", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
                else { 
                    EDFLocation = arg;
                }
            }

            try
            {
                InstallUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Si è verificato un errore fatale. Dettagli:\n\n" + ex.Message, "Errore non previsto", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

        }

        private async void InstallUpdate()
        {
            if (!System.IO.File.Exists(ProgramLocation + "\\update.zip"))
            {
                QuitAndStartEDF();
                return;
            }

            bool isRunning = true;

            while (isRunning)
            {
                isRunning = Process.GetProcessesByName("EraDeiFessi.exe").Count() > 0;
                await WaitAsync(1500);
            }

            tbMessage.Text = "Installazione della nuova versione";
            tbMessage.Foreground = Brushes.Green;

            await DecompressUpdateAsync();

            System.IO.File.Delete(ProgramLocation + "\\update.zip");
            QuitAndStartEDF();
        }

        private void QuitAndStartEDF()
        {
            if (System.IO.File.Exists(EDFLocation + "\\EraDeiFessi.exe"))
            {
                System.Diagnostics.Process.Start(EDFLocation + "\\EraDeiFessi.exe");
            }
            Application.Current.Shutdown();
        }

        public Task WaitAsync(int msec)
        {
            var task = Task.Factory.StartNew(() => Wait(msec));
            return task;
        }

        private void Wait(int msec)
        {
            System.Threading.Thread.Sleep(msec);
        }


        public Task DecompressUpdateAsync()
        {
            var task = Task.Factory.StartNew(() => DecompressUpdate());
            return task;
        }

        private void DecompressUpdate()
        {
            try
            {
                //clearing old plugins (in case some hosters are no longer working/supported)
                foreach (var item in Directory.GetFiles(EDFLocation + "\\plugins"))
                {
                    File.Delete(item);
                }

                //full wipe (can only be done in non-legacy mode, when the updater is not in the program directory)
                if (!legacymode)
                {
                    foreach (var item in Directory.GetFiles(EDFLocation))
                    {
                        File.Delete(item);
                    }
                }

                //MessageBox.Show("Files deleted");

                using (ZipFile zip1 = new ZipFile(ProgramLocation + "\\update.zip"))
                {
                    foreach (var item in zip1.Entries)
                    {
                        if (!legacymode || (!item.FileName.ToLower().Contains("edfupdater") && !item.FileName.ToLower().Contains("ionic.zip")))
                            try
                            {
                                item.Extract(EDFLocation, ExtractExistingFileAction.OverwriteSilently);
                            }
                            catch (Exception)
                            {
                            }
                            
                    }
                }
            }
            catch (Exception)
            {
                return;
            }

        }
    }
}
