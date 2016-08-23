using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace EraDeiFessi.Updater
{
    public class Updater
    {
        public static Task<CheckForUpdatesResponse> CheckForUpdatesAsync()
        {
            var task = Task.Factory.StartNew(() => CheckForUpdates());
            return task;
        }

        public static CheckForUpdatesResponse CheckForUpdates()
        {
            CheckForUpdatesResponse resp = new CheckForUpdatesResponse();

            try
            {
                using (WebClient Client = new WebClient())
                {


                    var datastream = new MemoryStream(Client.DownloadData(Constants.ReleaseManifestUrl));

                    XmlSerializer serializer = new XmlSerializer(typeof(ReleaseInfoCollection));

                    var manifest = (ReleaseInfoCollection)serializer.Deserialize(datastream);
                    datastream.Close();

                    resp.ReleaseManifest = manifest.releases;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return resp;
        }


        public static Task<bool> DownloadUpdateAsync()
        {
            var task = Task.Factory.StartNew(() => DownloadUpdate());
            return task;
        }

        public static bool DownloadUpdate()
        {
            try
            {
                if (!Directory.Exists(Constants.UpdateAppdataFolder))
                    Directory.CreateDirectory(Constants.UpdateAppdataFolder);

                using (WebClient Client = new WebClient())
                {
                    Client.DownloadFile(Constants.LatestReleasePackageUrl, Constants.UpdateAppdataFolder + "\\update.zip");
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }


        public static void DecompressUpdater()
        {
            string updatepack = Constants.UpdateAppdataFolder + "\\update.zip";
            if (System.IO.File.Exists(updatepack))
            {
                try
                {
                    using (ZipFile zip1 = new ZipFile(updatepack))
                    {
                        foreach (var item in zip1.Entries)
                        {
                            if (item.FileName.ToLower().Contains("edfupdater") || item.FileName.ToLower().Contains("ionic.zip"))
                                item.Extract(Constants.UpdateAppdataFolder, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }


        public static void InvokeUpdater()
        {
            var psi = new ProcessStartInfo();
            psi.FileName = Constants.UpdateAppdataFolder + "\\EDFUpdater.exe";
            psi.Arguments = "\"EDFPath=" + Constants.ProgramLocation + "\"";
            psi.Verb = "runas";
            try
            {
                var process = new Process();
                process.StartInfo = psi;
                process.Start();
                //process.WaitForExit();
            }
            catch (Exception)
            {
                MessageBox.Show("Errore: impossibile aggiornare EraDeiFessi perchè non hai concesso i privilegi di amministrazione. Questi sono necessari all'installazione degli aggiornamenti.\n\n" +
                    "Verrà fatto un nuovo tentativo al prossimo avvio dell'applicazione.", "Aggiornamento fallito", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
